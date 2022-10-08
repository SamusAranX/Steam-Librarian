using System.Diagnostics;
using SteamROMLibrarian.Serialization;

namespace SteamROMLibrarian
{
	/// <summary>
	/// The class containing the actual top level logic
	/// </summary>
	internal class Librarian
	{
		private static (string SteamLocation, string SteamUserIDFolder) Preflight(string? steamUserID = null)
		{
			var steamLocation = Utils.Steam.GetSteamPath();

			if (steamUserID == null)
			{
				var steamUserIDs = Utils.Steam.GetSteamUserIDs();
				switch (steamUserIDs.Length)
				{
					case 0:
						throw new NoUserIDsFoundException("Couldn't find any user IDs.");
					case 1:
						steamUserID = steamUserIDs[0];
						break;
					default:
						// TODO: add selection menu
						steamUserID = steamUserIDs[0];
						break;
				}
			}

			var userIDFolder = Path.Join(steamLocation, "userdata", steamUserID);
			if (!Directory.Exists(userIDFolder))
			{
				throw new InvalidUserIDException($"Invalid user ID {steamUserID}.");
			}

			return (Path.GetFullPath(steamLocation), Path.GetFullPath(userIDFolder));
		}

		public static void PrepareLibrary(string? steamUserID, string libraryPath, bool overwriteLibraryFile)
		{
			var (steamLocation, userIDFolder) = Preflight(steamUserID);
			var steamShortcutsPath = Path.Join(userIDFolder, "config", "shortcuts.vdf");
			var shortcutsDeckPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "shortcutsDeck.vdf");

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

		public static void WriteLibrary(string? steamUserID, string libraryPath)
		{
			var (steamLocation, userIDFolder) = Preflight(steamUserID);
			var steamShortcutsPath = Path.Join(userIDFolder, "config", "shortcuts.vdf");
			var steamShortcutsBackupPath = Path.Join(userIDFolder, "config", "shortcuts.bak");
			var steamShortcutsPathDebug = Path.Join(userIDFolder, "config", "shortcutsDebug.vdf");

			var steamProcesses = Process.GetProcessesByName("steam");
			if (steamProcesses.Length > 0)
			{
				Console.WriteLine("It looks like Steam is currently running! Please exit Steam completely before doing this. Your changes will be visible once you restart Steam.");
				return;
			}

			//return;

			// TODO: add custom artwork handling
			// TODO: read LastPlayedTime from shortcuts.vdf and use that instead of reusing the value from library.json

			// TODO: make preexistingShortcuts a list of appID and app names. app names are only for the user while IDs are used to look up shortcuts in shortcuts.vdf to reuse them when rewriting

			var library = GameLibrary.Load(libraryPath);
			Console.WriteLine("Library loaded successfully");

			var shortcutsVDF = ShortcutsVDF.Load(steamShortcutsPath);
			Console.WriteLine("shortcuts.vdf loaded successfully");

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
				if (!library.ContainsPreexisting(s.AppID.ToString()))
					newShortcuts.Add(new(s.AppID.ToString(), s.AppName));
			}

			library.PreexistingShortcuts.RemoveAll(s => deletedShortcuts.Contains(s));
			library.PreexistingShortcuts.AddRange(newShortcuts);

			// at this point, PreexistingShortcuts should only contain pointers to shortcuts that actually exist within shortcuts.vdf
			var existingShortcuts = new List<Shortcut>();
			foreach (var ps in library.PreexistingShortcuts)
			{
				existingShortcuts.Add(shortcutsVDF.GetByID(ps.AppID)!);
			}

			Console.WriteLine("Housekeeping completed");
			library.Save(libraryPath);

			var newShortcutsVDF = new ShortcutsVDF()
			{
				Shortcuts = existingShortcuts,
			};

			foreach (var (categoryName, category) in library.Categories)
			{
				if (!library.Launchers.ContainsKey(category.DefaultLauncher))
				{
					Console.WriteLine($"Can't find default launcher for category {categoryName}! Skipping category");
					continue;
				}

				var launcher = library.Launchers[category.DefaultLauncher];
				if (launcher.Executable.Trim() == "")
				{
					Console.WriteLine($"Default launcher {category.DefaultLauncher} for category {categoryName} is missing an executable path! Skipping category");
					continue;
				}

				foreach (var entry in category.Entries)
				{
					if (!uint.TryParse(entry.Metadata.AppID, out var appID))
					{
						Console.WriteLine($"Entry {categoryName}->{entry.Name} is missing its metadata or has an invalid app ID! Skipping entry");
						continue;
					}

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

					var exeDir = new FileInfo(launcher.Executable).DirectoryName;
					if (exeDir == null)
					{
						Console.WriteLine($"Can't determine the containing directory for launcher {launcherName}! Skipping entry");
						continue;
					}

					var argsList = new List<string>();
					argsList.Add(launcher.Executable);
					if (launcher.Arguments.Trim() != "")
						argsList.Add(launcher.Arguments);

					if (entry.Path != null && entry.Path.Trim() != "")
						argsList.Add(entry.Path);

					var shortcut = new Shortcut()
					{
						AppID = appID,
						AppName = entry.Name,
						Exe = string.Join(" ", argsList),
						StartDir = exeDir,
						LastPlayTimeUnix = entry.Metadata.LastPlayTimeUnix,
						OpenVR = entry.VR,
						Tags = new List<string> { categoryName },
					};
					newShortcutsVDF.Shortcuts.Add(shortcut);
				}
			}

			Console.WriteLine($"Creating backup at {steamShortcutsBackupPath}");
			File.Copy(steamShortcutsPath, steamShortcutsBackupPath, true);
			Console.WriteLine($"Backup created.");

			Console.WriteLine($"Writing library to {steamShortcutsPathDebug}");
			newShortcutsVDF.Save(steamShortcutsPathDebug);
			Console.WriteLine($"Library written.");

			// TODO: implement custom artwork loading/copying

			Console.WriteLine($"Restart Steam to reload your changes.");
		}

		public static void WriteExampleLibrary(string libraryPath)
		{
			Console.WriteLine(GameLibrary.ExampleLibrary.ToJSON());
		}
	}
}
