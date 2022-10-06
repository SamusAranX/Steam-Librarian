using System.Text.Json;
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

			return (steamLocation, userIDFolder);
		}

		public static void PrepareLibrary(string? steamUserID, string libraryPath, bool overwriteLibraryFile)
		{
			var (steamLocation, userIDFolder) = Preflight(steamUserID);
			var shortcutsPath = Path.Join(userIDFolder, "config", "shortcuts.vdf");

			// var shortcutsPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "shortcuts.vdf");
			var shortcuts2Path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "shortcuts.vdf");

			//if (File.Exists(libraryPath) && !overwriteLibraryFile)
			//{
			//	Console.WriteLine("The library JSON file already exists. Please specify --overwrite if you really intended to regenerate it.");
			//	return;
			//}

			var library = new GameLibrary();
			var shortcuts = new ShortcutsVDF(shortcutsPath);

			library.LeaveUntouched.AddRange(shortcuts);

			shortcuts.Save(shortcuts2Path);

			foreach (var shortcut in shortcuts)
			{
				library.LeaveUntouched.Add(shortcut);
				var appID = shortcut.AppID.ToString();
				var exe = shortcut.Exe!;
				var appName = shortcut.AppName!;
				Console.WriteLine($"{appID}: {appName} -> {exe}");
			}

			//library.Serialize(libraryPath);
		}

		public static void WriteLibrary(string? steamUserID, string libraryPath)
		{
			var (steamLocation, userIDFolder) = Preflight(steamUserID);

			// TODO: add the actual library writing part

			var library = GameLibrary.Deserialize(libraryPath);

			// all ROMEntry objects in the library now have AppIDs

			// write library back to the JSON file so new AppIDs are persisted
			library.Serialize(libraryPath);
		}

		public static void WriteExampleLibrary(string libraryPath)
		{
			var exampleLibrary = GameLibrary.ExampleLibrary;
			var exampleLibraryPath = Path.Join(Path.GetDirectoryName(libraryPath), "libraryExample.json");

			using (var fs = File.OpenWrite(exampleLibraryPath))
			{
				JsonSerializer.Serialize(fs, exampleLibrary, JSON.Options);
			}
		}
	}
}
