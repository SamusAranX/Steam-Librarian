using System.Diagnostics;
using System.Text.Json;
using SteamROMLibrarian.Serialization;
using SteamROMLibrarian.Utils;

namespace SteamROMLibrarian;

/// <summary>
/// The class containing the actual top level logic
/// </summary>
internal class Librarian
{
	private static (string SteamLocation, string SteamUserIDFolder) Preflight(string? steamUserID = null)
	{
		var steamLocation = Steam.GetSteamPath();

		if (steamUserID == null)
		{
			var steamUserIDs = Steam.GetSteamUserIDs();
			switch (steamUserIDs.Length)
			{
				case 0:
					Console.WriteLine("Couldn't find any Steam users.");
					Environment.Exit(1);
					break;
				case 1:
					steamUserID = steamUserIDs[0];
					break;
				default:
					var steamLoginUsersPath = Path.Join(steamLocation, "config", "loginusers.vdf");
					var loginUsers = SteamLoginUsersVDF.Load(steamLoginUsersPath);

					Console.WriteLine("Found more than one Steam user:");
					Console.WriteLine("----------");
					foreach (var userID in steamUserIDs)
					{
						var loginUser = loginUsers.SingleOrDefault(u => u.UserID == userID);
						if (loginUser != null)
							Console.WriteLine($"{userID}: {loginUser.PersonaName}");
						else
							Console.WriteLine(userID);
					}

					Console.WriteLine("----------");
					Console.WriteLine("Please use --user-id to specify one of these.");
					Environment.Exit(1);
					break;
			}
		}

		var userIDFolder = Path.Join(steamLocation, "userdata", steamUserID);
		if (!Directory.Exists(userIDFolder))
			throw new InvalidUserIDException($"Invalid user ID {steamUserID}.");

		return (Path.GetFullPath(steamLocation), Path.GetFullPath(userIDFolder));
	}

	public static void PrepareLibrary(string? steamUserID, string libraryPath, bool overwriteLibraryFile)
	{
		var (steamLocation, userIDFolder) = Preflight(steamUserID);
		var steamShortcutsPath = Path.Join(userIDFolder, "config", "shortcuts.vdf");

		if (File.Exists(libraryPath) && !overwriteLibraryFile)
		{
			Console.WriteLine("The library JSON file already exists. Please specify --overwrite if you really intended to regenerate it.");
			return;
		}

		var library = new GameLibrary();
		var shortcuts = ShortcutsVDF.Load(steamShortcutsPath);

		library.PreexistingShortcuts = shortcuts.Select(s => new ShortcutPointer(s.AppID.ToString(), s.AppName)).ToList();
		library.Save(libraryPath);
	}

	public static void CheckLibrary(string libraryPath, bool showEntryPaths, bool fixLibrary)
	{
		GameLibrary library;
		try
		{
			library = GameLibrary.Load(libraryPath);
			Console.WriteLine("Library loaded successfully");
			Console.WriteLine();
		}
		catch (JsonException e)
		{
			Console.WriteLine(e.Message);
			Debug.WriteLine(e);
			return;
		}

		var libraryDir = Path.GetDirectoryName(libraryPath)!;

		foreach (var (categoryName, category) in library.Categories)
		{
			Console.WriteLine($"##### Category \"{categoryName}\"");

			ROMLauncher? launcher = null;
			if (category.DefaultLauncher != null)
			{
				if (!library.Launchers.ContainsKey(category.DefaultLauncher))
				{
					Console.WriteLine($"Can't find default launcher for category {categoryName}! Skipping category");
					continue;
				}

				launcher = library.Launchers[category.DefaultLauncher];
				if (launcher.Executable.Trim() == "")
				{
					Console.WriteLine($"Default launcher {category.DefaultLauncher} for category {categoryName} is missing an executable path! Skipping category");
					continue;
				}
			}

			foreach (var entry in category.Entries)
			{
				Console.Write($"--- {entry.Name}");
				if (entry.NoRoot)
					Console.WriteLine(" (no root)");
				else
				{
					var entryPath = entry.Executable(category.RootDirectory, launcher);
					if (!File.Exists(entryPath))
						Console.WriteLine(" (NOT FOUND)");
					else
						Console.WriteLine();

					if (showEntryPaths)
						Console.WriteLine(entryPath);
				}

				var imageTypes = new[] { ROMEntry.ImageType.Grid, ROMEntry.ImageType.Poster, ROMEntry.ImageType.Hero, ROMEntry.ImageType.Logo, ROMEntry.ImageType.Icon };
				var imageExts = new[] {"png", "jpg", "jpeg"};
				foreach (var imageType in imageTypes)
				{
					var imagePath = entry.GetImagePath(imageType);
					if (imagePath == null)
						continue;

					if (!Path.IsPathRooted(imagePath))
						imagePath = Path.Join(libraryDir, imagePath);

					string? newImagePath = null;

					var imageExists = File.Exists(imagePath);
					if (!imageExists)
					{
						Console.WriteLine($"Can't find {imageType} image \"{imagePath}\"");

						var imageExt = Path.GetExtension(imagePath);
						foreach (var ext in imageExts.SkipWhile(e => e == imageExt))
						{
							var testImageExtPath = Path.ChangeExtension(imagePath, ext);
							if (File.Exists(testImageExtPath))
							{
								newImagePath = Path.GetRelativePath(libraryDir, testImageExtPath).Replace("\\", "/");
								break;
							}
						}
					}

					if (!string.IsNullOrEmpty(newImagePath))
					{
						Console.WriteLine($"Found {imageType} image as \"{Path.GetFileName(newImagePath)}\"");
					}

					if (fixLibrary && !imageExists && imagePath != newImagePath)
					{
						Console.WriteLine("Correcting library file");
						entry.SetImagePath(imageType, newImagePath);
					}
				}
			}

			Console.WriteLine();
		}

		if (fixLibrary)
		{
			Console.WriteLine("Writing fixed library…");
			library.Save(libraryPath);
			Console.WriteLine("Done.");
		}
	}

	public static void WriteLibrary(string? steamUserID, string libraryPath)
	{
		var (steamLocation, userIDFolder) = Preflight(steamUserID);
		var steamShortcutsPath = Path.Join(userIDFolder, "config", "shortcuts.vdf");
		var steamShortcutsBackupPath = Path.Join(userIDFolder, "config", "shortcuts.bak");

		//var steamShortcutsPathDebug = Path.Join(userIDFolder, "config", "shortcutsDebug.vdf");
		var steamGridPath = Path.Join(userIDFolder, "config", "grid");
		var libraryDir = Path.GetDirectoryName(libraryPath);

		if (Steam.IsSteamRunning())
		{
			Console.WriteLine("It looks like Steam is currently running! Please exit Steam completely before doing this. Your changes will be visible once you restart Steam.");
			return;
		}

		var library = GameLibrary.Load(libraryPath);
		Console.WriteLine("Library loaded successfully");

		var shortcutsVDF = new ShortcutsVDF();
		if (File.Exists(steamShortcutsPath))
		{
			shortcutsVDF = ShortcutsVDF.Load(steamShortcutsPath);
			Console.WriteLine("shortcuts.vdf loaded successfully");
		}
		else
			Console.WriteLine("No shortcuts.vdf found. Generating new file");

		Console.WriteLine("Doing housekeeping...");

		var deletedShortcuts = new List<ShortcutPointer>(); // shortcuts in library.json that are no longer present in shortcuts.vdf
		foreach (var s in library.PreexistingShortcuts)
		{
			var vdfShortcut = shortcutsVDF.GetByID(s.AppID);
			if (vdfShortcut == null)
				deletedShortcuts.Add(s);
		}

		var newShortcuts = new List<ShortcutPointer>(); // shortcuts in shortcuts.vdf that are not in library.json yet
		foreach (var s in shortcutsVDF)
		{
			if (!library.ContainsPreexisting(s.AppID.ToString()) && !library.Contains(s.AppID.ToString()))
				newShortcuts.Add(new ShortcutPointer(s.AppID.ToString(), s.AppName));
		}

		library.PreexistingShortcuts.RemoveAll(s => deletedShortcuts.Contains(s));
		library.PreexistingShortcuts.AddRange(newShortcuts);

		// at this point, PreexistingShortcuts should only contain pointers to shortcuts that actually exist within shortcuts.vdf
		var existingShortcuts = new List<Shortcut>();
		foreach (var ps in library.PreexistingShortcuts)
			existingShortcuts.Add(shortcutsVDF.GetByID(ps.AppID)!);

		var newShortcutsVDF = new ShortcutsVDF
		{
			Shortcuts = existingShortcuts,
		};

		foreach (var (categoryName, category) in library.Categories)
		{
			ROMLauncher? launcher = null;
			if (category.DefaultLauncher != null)
			{
				if (!library.Launchers.ContainsKey(category.DefaultLauncher))
				{
					Console.WriteLine($"Can't find default launcher for category {categoryName}! Skipping category");
					continue;
				}

				launcher = library.Launchers[category.DefaultLauncher];
				if (launcher.Executable.Trim() == "")
				{
					Console.WriteLine($"Default launcher {category.DefaultLauncher} for category {categoryName} is missing an executable path! Skipping category");
					continue;
				}
			}

			foreach (var entry in category.Entries)
			{
				if (!uint.TryParse(entry.Metadata.AppID, out var appIDInt))
				{
					Console.WriteLine($"Entry {categoryName}->{entry.Name} is missing its metadata or has an invalid app ID! Skipping entry");
					continue;
				}

				var appID = new AppID(entry.Name, appIDInt);

				var launcherName = category.DefaultLauncher;
				if (entry.Launcher != null)
				{
					if (!library.Launchers.ContainsKey(entry.Launcher))
					{
						Console.WriteLine($"Can't find custom launcher for entry {categoryName}->{entry.Name}! Skipping entry");
						continue;
					}

					launcherName = entry.Launcher;
					launcher = library.Launchers[launcherName];
					if (launcher.Executable.Trim() == "")
					{
						Console.WriteLine($"Custom launcher {launcherName} for entry {categoryName}->{entry.Name} is missing an executable path! Skipping entry");
						continue;
					}
				}

				var exePath = entry.Executable(category.RootDirectory, launcher);

				var startDir = "";
				if (launcher != null)
				{
					startDir = new FileInfo(launcher.Executable).DirectoryName;
					if (startDir == null)
						throw new ArgumentException($"Launcher {launcherName} does not point to a valid executable");
				}
				else
				{
					startDir = new FileInfo(exePath).DirectoryName;
					if (startDir == null)
						throw new ArgumentException($"Entry {categoryName}->{entry.Name} does not point to a valid executable");
				}

				var imageTypes = new[] { ROMEntry.ImageType.Grid, ROMEntry.ImageType.Poster, ROMEntry.ImageType.Hero, ROMEntry.ImageType.Logo, ROMEntry.ImageType.Icon };
				var iconPath = "";
				foreach (var imageType in imageTypes)
				{
					var imagePath = entry.GetImagePath(imageType);
					if (imagePath == null)
						continue;

					if (!Path.IsPathRooted(imagePath))
						imagePath = Path.Join(libraryDir, imagePath);

					if (!File.Exists(imagePath))
					{
						Console.WriteLine($"Can't find {imageType} image \"{imagePath}\"");
						continue;
					}

					// if imagePath is not null, imageName won't be either, so unwrapping is safe
					var imageName = entry.GetSteamImageName(imageType)!;
					var steamImagePath = Path.Join(steamGridPath, imageName);

					if (imageType == ROMEntry.ImageType.Icon)
						iconPath = steamImagePath;

					File.Copy(imagePath, steamImagePath, true);
				}

				// read LastPlayedTime and tags from shortcuts.vdf and use those if possible
				// instead of reusing the values from library.json
				var lastPlayTime = entry.Metadata.LastPlayTimeUnix;
				var shortcutTags = new List<string> { categoryName };
				var previouslyExported = shortcutsVDF.GetByID(appID.ShortcutID);
				if (previouslyExported != null)
				{
					lastPlayTime = previouslyExported.LastPlayTimeUnix;
					shortcutTags = previouslyExported.Tags;

					// make sure the entry is put into the collection given in library.json
					if (!shortcutTags.Contains(categoryName))
						shortcutTags.Add(categoryName);
				}

				var shortcut = new Shortcut
				{
					AppID = appIDInt,
					AppName = entry.Name,
					Exe = exePath,
					StartDir = startDir,
					Icon = iconPath,
					LastPlayTimeUnix = lastPlayTime,
					OpenVR = entry.VR,
					Tags = shortcutTags,
				};
				newShortcutsVDF.Shortcuts.Add(shortcut);
			}
		}

		Console.WriteLine("Housekeeping completed.");
		library.Save(libraryPath);

		if (File.Exists(steamShortcutsPath))
		{
			Console.WriteLine($"Creating backup at {steamShortcutsBackupPath}");
			File.Copy(steamShortcutsPath, steamShortcutsBackupPath, true);
			Console.WriteLine("Backup created.");
		}

		Console.WriteLine($"Writing library to {steamShortcutsPath}");
		newShortcutsVDF.Save(steamShortcutsPath);
		Console.WriteLine("Library written.");

		Console.WriteLine("Restart Steam to reload your changes.");
	}

	public static void WriteExampleLibrary()
	{
		Console.WriteLine(GameLibrary.ExampleLibrary.ToJSON());
	}

	public static void ResetCollections()
	{
		if (!Steam.IsSteamRunning())
		{
			Console.WriteLine("This command only works when Steam is running. Please start Steam, then try this again.");
			return;
		}

		var psi = new ProcessStartInfo("steam://resetcollections")
		{
			UseShellExecute = true,
		};
		Process.Start(psi);
	}
}
