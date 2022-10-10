using System.Text;
using Force.Crc32;

namespace SteamROMLibrarian.Utils
{
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

		public AppID(string appName, uint appID)
		{
			this.AppName = appName;

			this.ShortcutID = appID.ToString();
			this.LegacyID = (((ulong)appID << 32) | BPM).ToString();
		}
	}
}
