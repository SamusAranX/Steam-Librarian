using System.Globalization;

namespace VDFLib
{
	[Serializable]
	public class ShortcutTag
	{
		public uint Index { get; set; }
		public string Name { get; set; }
	}

	[Serializable]
	public class Shortcut
	{
		public string Index { get; set; }
		public uint AppID { get; set; }
		public string AppName { get; set; }
		public string Exe { get; set; }
		public string StartDir { get; set; }
		public string Icon { get; set; }
		public string ShortcutPath { get; set; }
		public string LaunchOptions { get; set; }
		public uint IsHidden { get; set; }
		public uint AllowDesktopConfig { get; set; }
		public uint OpenVR { get; set; }
		public uint Devkit { get; set; }
		public uint DevkitGameID { get; set; }
		public uint DevkitOverrideAppID { get; set; }
		public DateTime LastPlayTime { get; set; }
		public string FlatpakAppID { get; set; }
		public ShortcutTag[] Tags { get; set; }
	}

	public class Shortcuts
	{
		private const byte NUL = 0x00;
		private const byte STRING_VALUE = 0x01;
		private const byte INT_VALUE = 0x02;
		private const byte END = 0x08;

		public List<Shortcut> ShortcutList { get; set; } = new List<Shortcut>();

		private KVObject kvObject;

		public Shortcuts(string filename)
		{
			using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

			var deserializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Binary);
			this.kvObject = deserializer.Deserialize(fs);
		}

		public void DisplayTree(KVObject? kv = null, int level = 0)
		{
			if (kv == null)
				kv = this.kvObject;

			var padding = new string(' ', level);
			Console.WriteLine("{0}{1} → {2} ({3})", padding, kv.Name, kv.Value.ToString(CultureInfo.InvariantCulture), kv.Value.ValueType.ToString());
			foreach (var child in kv)
			{
				this.DisplayTree(child, level + 1);
			}
		}
	}
}
