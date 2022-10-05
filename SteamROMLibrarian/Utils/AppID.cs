using Force.Crc32;

namespace SteamROMLibrarian.Utils
{
	internal class AppID
	{
		private static ulong GenerateAppIDInt(string binaryName, string appName)
		{
			var key = $"{binaryName}{appName}";
			ulong id = Crc32Algorithm.Compute(System.Text.Encoding.UTF8.GetBytes(key)) | 0x80000000;
			return id << 32 | 0x02000000;
		}

		/// <summary>
		/// Used for Big Picture grids
		/// </summary>
		public static string GenerateAppID(string binaryName, string appName)
		{
			return GenerateAppIDInt(binaryName, appName).ToString();
		}

		/// <summary>
		/// Used for non Big Picture grids
		/// </summary>
		public static string GenerateShortAppID(string binaryName, string appName)
		{
			return (GenerateAppIDInt(binaryName, appName) >> 32).ToString();
		}

		/// <summary>
		/// Used as app ID in shortcuts.vdf
		/// </summary>
		public static string GenerateShortcutID(string binaryName, string appName)
		{
			return ((GenerateAppIDInt(binaryName, appName) >> 32) - 0x100000000).ToString();
		}

		/// <summary>
		/// Convert from app ID to short app ID
		/// </summary>
		public static string ShortenAppID(ulong longID)
		{
			return (longID >> 32).ToString();
		}

		/// <summary>
		/// Convert from short app ID to app ID
		/// </summary>
		public static string LengthenAppID(ulong shortID)
		{
			return (shortID << 32).ToString();
		}
	}
}
