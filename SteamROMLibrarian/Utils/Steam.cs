using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace SteamROMLibrarian.Utils;

internal class Steam
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

			if (key?.GetValue("SteamPath") is string steamPath && Directory.Exists(Path.Join(steamPath, "appcache")))
				return steamPath;

			throw new SteamPathNotFoundException("Can't find Steam path");
		}

		if (!isLinux && !isMac)
			throw new PlatformNotSupportedException();
		
		// platform is linux or mac
		var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		var paths = isLinux ? new[] { ".steam", ".steam/steam", ".steam/root", ".local/share/Steam" } : new[] { "Library/Application Support/Steam" };

		return paths
			.Select(path => Path.Join(home, path))
			.FirstOrDefault(steamPath => Directory.Exists(Path.Join(steamPath, "appcache")), null) ?? throw new SteamPathNotFoundException("Can't find Steam path");
	}

	public static string[] GetSteamUserIDs()
	{
		var steamPath = GetSteamPath();
		try
		{
			var userIDFolders = Directory.EnumerateDirectories(Path.Join(steamPath, "userdata")).ToArray();
			return userIDFolders.Select(x => new DirectoryInfo(x).Name).ToArray();
		}
		catch (Exception)
		{
			return Array.Empty<string>();
		}
	}

	public static bool IsSteamRunning()
	{
		var steamProcesses = Process.GetProcessesByName("steam");
		return steamProcesses.Length > 0;
	}
}
