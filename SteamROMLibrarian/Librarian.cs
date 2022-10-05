using AppInfoParser;

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

		public static void ReadLibrary(string? steamUserID, string libraryPath)
		{
			var (steamLocation, userIDFolder) = Preflight(steamUserID);

			var appInfo = new AppInfo();
			appInfo.Read(Path.Join(steamLocation, "appcache", "appinfo.vdf"));

			var appList = appInfo.Apps.FindAll(app => Directory.Exists(Path.Join(userIDFolder, app.AppID.ToString())));

			Console.WriteLine($"{appList.Count} apps");
			Console.WriteLine("-----");

			foreach (var app in appList)
			{
				//var userdataFolder = Path.Join(steamLocation, "userdata", steamUserID, app.AppID.ToString());

				Console.WriteLine(app.AppID);

				//Console.WriteLine(app.Data);
				//Console.WriteLine("-----");
			}
		}

		public static void WriteLibrary(string? steamUserID, string libraryPath)
		{
			var (steamLocation, userIDFolder) = Preflight(steamUserID);

			// TODO: add the actual library writing part
			throw new NotImplementedException("not yet");
		}
	}
}
