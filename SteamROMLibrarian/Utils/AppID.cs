using System.Text;
using Force.Crc32;

namespace SteamROMLibrarian.Utils
{
	[Obsolete("Use AppID instead", true)]
	internal class AppIDOld
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

	internal class AppID
	{
		private const uint TOP = 0x8000_0000; // sets the 32nd bit to 1 
		private const ulong BPM = 0x0200_0000; // used in big picture mode
		private const ulong SHC = 0x1_0000_0000; // constant that's subtracted to get the shortcut ID
		private const ulong B32 = 0xFFFF_FFFF;

		public string? Exe { get; }
		public string AppName { get; }

		public string ShortcutID { get; }

		public string LegacyID { get; }

		public AppID(string exe, string appName)
		{
			this.Exe = exe;
			this.AppName = appName;

			var crc = Crc32Algorithm.Compute(Encoding.UTF8.GetBytes(exe + appName));
			this.ShortcutID = ((ulong)crc | TOP).ToString();
			this.LegacyID = ((((ulong)crc | TOP) << 32) | BPM).ToString();
		}

		public AppID(string appName, string shortcutID, string legacyID)
		{
			this.AppName = appName;
			this.ShortcutID = shortcutID;
			this.LegacyID = legacyID;
		}

		public string GridImagePath(string gridDir, string ext) => Path.Join(gridDir, $"{this.ShortcutID}.{ext}");
		public string HeroImagePath(string gridDir, string ext) => Path.Join(gridDir, $"{this.ShortcutID}_hero.{ext}");
		public string IconImagePath(string gridDir, string ext) => Path.Join(gridDir, $"{this.ShortcutID}_icon.{ext}");
		public string LogoImagePath(string gridDir, string ext) => Path.Join(gridDir, $"{this.ShortcutID}_logo.{ext}");
		public string PosterImagePath(string gridDir, string ext) => Path.Join(gridDir, $"{this.ShortcutID}p.{ext}");
		public string BigPictureGridImagePath(string gridDir, string ext) => Path.Join(gridDir, $"{this.LegacyID}.{ext}");
	}
}
