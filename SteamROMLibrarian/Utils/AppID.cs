using System.Text;
using Force.Crc32;

namespace SteamROMLibrarian.Utils
{
	internal class AppID
	{
		private const ulong TOP = 0x8000_0000; // sets the 32nd bit to 1 
		private const ulong BPM = 0x0200_0000; // used in big picture mode
		private const ulong SHC = 0x1_0000_0000; // constant that's subtracted to get the shortcut ID
		private const ulong B32 = 0xFFFF_FFFF;

		private static readonly Encoding UTF8 = Encoding.UTF8;

		//public static string GenerateGridAppID(string exe, string appName)
		//{
		//	var nameTargetBytes = UTF8.GetBytes(exe + appName);
		//	var crc = Crc32Algorithm.Compute(nameTargetBytes);
		//	var gameId = crc | TOP;

		//	return gameId.ToString();
		//}

		private static ulong GenerateAppIDInt(string exe, string appName)
		{
			var key = exe + appName;
			var id = Crc32Algorithm.Compute(UTF8.GetBytes(key)) | TOP;
			return id << 32 | BPM;
		}

		/// <summary>
		/// Used for Big Picture grids
		/// </summary>
		public static string GenerateLegacyAppID(string exe, string appName)
		{
			return GenerateAppIDInt(exe, appName).ToString();
		}

		/// <summary>
		/// Used for non Big Picture grids
		/// </summary>
		public static string GenerateAppID(string exe, string appName)
		{
			return (GenerateAppIDInt(exe, appName) >> 32).ToString();
		} 

		/// <summary>
		/// Used as app ID in shortcuts.vdf
		/// </summary>
		public static string GenerateShortcutID(string exe, string appName)
		{
			return ((GenerateAppIDInt(exe, appName) >> 32) - SHC).ToString();
		}
	}
}
