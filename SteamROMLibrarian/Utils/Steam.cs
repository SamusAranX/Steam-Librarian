using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace SteamROMLibrarian.Utils
{
	public class Steam
	{
		public static string GetSteamPath()
		{
			var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
			var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
			var isMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

			if (isWindows)
			{
				var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Valve\\Steam") ??
				          RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
					          .OpenSubKey("SOFTWARE\\Valve\\Steam");

				if (key?.GetValue("SteamPath") is string steamPath)
				{
					return steamPath;
				}
			}
			else if (isLinux || isMac)
			{
				var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
				var paths = isLinux ? 
					new[] { ".steam", ".steam/steam", ".steam/root", ".local/share/Steam" } : 
					new[] { "Library/Application Support/Steam" };

				return paths
					.Select(path => Path.Join(home, path))
					.FirstOrDefault(steamPath => Directory.Exists(Path.Join(steamPath, "appcache"))) ?? throw new SteamPathNotFoundException("Can't find Steam path");
			}

			throw new PlatformNotSupportedException();
		}

		public static string[] GetSteamUserIDs()
		{
			var steamPath = GetSteamPath();
			var userIDFolders = Directory.EnumerateDirectories(Path.Join(steamPath, "userdata")).ToArray();
			return userIDFolders.Select(x => new DirectoryInfo(x).Name).ToArray();
		}
	}
}
