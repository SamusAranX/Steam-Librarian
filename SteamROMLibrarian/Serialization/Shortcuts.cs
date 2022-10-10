using System.Collections;
using System.Text.Json.Serialization;
using ValveKeyValue;

namespace SteamROMLibrarian.Serialization
{
	[Serializable]
	internal class Shortcut
	{
		[KVProperty("appid")]
		[JsonIgnore]
		public int AppIDInt { get; set; }

		[JsonPropertyName("AppID")]
		[KVIgnore]
		public uint AppID
		{
			get => (uint)this.AppIDInt;
			set => this.AppIDInt = (int)value;
		}

		public string AppName { get; set; } = "";

		private string _exe = "";
		private string _startDir = "";
		private string? _icon;

		public string Exe
		{
			get => this._exe;
			set
			{
				if (!value.IsQuoted())
					value = value.ToQuotedString();

				this._exe = value;
			}
		}

		public string StartDir
		{
			get => this._startDir;
			set
			{
				if (!value.IsQuoted())
					value = value.ToQuotedString();

				this._startDir = value;
			}
		}

		[KVProperty("icon")]
		public string? Icon
		{
			get => this._icon;
			set
			{
				if (value != null && !value.IsQuoted())
					value = value.ToQuotedString();

				this._icon = value;
			}
		}

		public string ShortcutPath { get; set; }
		public string LaunchOptions { get; set; }
		public bool IsHidden { get; set; }
		public bool AllowDesktopConfig { get; set; } = true;
		public bool AllowOverlay { get; set; } = true;
		public bool OpenVR { get; set; }
		public bool Devkit { get; set; }
		public string DevkitGameID { get; set; }
		public int DevkitOverrideAppID { get; set; }

		[KVProperty("LastPlayTime")]
		public int LastPlayTimeUnix { get; set; }

		[KVIgnore]
		[JsonIgnore]
		public DateTime LastPlayTime
		{
			get => new DateTime(1970, 1, 1).ToLocalTime().AddSeconds(this.LastPlayTimeUnix);
			set => this.LastPlayTimeUnix = (int)((DateTimeOffset)value).ToUnixTimeSeconds();
		}

		public string FlatpakAppID { get; set; } = "";

		[KVProperty("tags")]
		public List<string> Tags { get; set; } = new();

		public KVObject ToKVObject(int index)
		{
			var kv = new KVObject(index.ToString(), Array.Empty<KVObject>())
			{
				["appid"] = this.AppIDInt,
				["AppName"] = this.AppName,
				["Exe"] = this.Exe,
				["StartDir"] = this.StartDir,
				["icon"] = this.Icon ?? "",
				["ShortcutPath"] = this.ShortcutPath ?? "",
				["LaunchOptions"] = this.LaunchOptions ?? "",
				["IsHidden"] = this.IsHidden ? 1 : 0,
				["AllowDesktopConfig"] = this.AllowDesktopConfig ? 1 : 0,
				["AllowOverlay"] = this.AllowOverlay ? 1 : 0,
				["OpenVR"] = this.OpenVR ? 1 : 0,
				["Devkit"] = this.Devkit ? 1 : 0,
				["DevkitGameID"] = this.DevkitGameID ?? "",
				["DevkitOverrideAppID"] = this.DevkitOverrideAppID,
				["LastPlayTime"] = this.LastPlayTimeUnix,
				["FlatpakAppID"] = this.FlatpakAppID,
			};

			var tagsList = new List<KVObject>();
			for (var i = 0; i < this.Tags.Count; i++)
			{
				var kvTag = new KVObject(i.ToString(), this.Tags[i]);
				tagsList.Add(kvTag);
			}

			kv.Add(new KVObject("tags", tagsList));

			return kv;
		}

		public override string ToString()
		{
			return $"({this.AppID}) {this.AppName}";
		}
	}

	[Serializable]
	internal class ShortcutsVDF : IEnumerable<Shortcut>
	{
		[KVProperty("shortcuts")]
		public List<Shortcut> Shortcuts { get; set; } = new();

		#region IEnumerable

		public IEnumerator<Shortcut> GetEnumerator()
		{
			return this.Shortcuts.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.Shortcuts.GetEnumerator();
		}

		#endregion

		public Shortcut? GetByID(string appID)
		{
			foreach (var s in this)
			{
				if (appID == s.AppID.ToString())
					return s;
			}

			return null;
		}

		public bool Contains(string appID)
		{
			return this.GetByID(appID) != null;
		}

		public static ShortcutsVDF Load(string filename)
		{
			using var fs = File.OpenRead(filename);
			var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Binary);

			try
			{
				var kvObject = kv.Deserialize<List<Shortcut>>(fs);
				var vdf = new ShortcutsVDF
				{
					Shortcuts = kvObject,
				};
				return vdf;
			}
			catch (Exception)
			{
				throw new InvalidVDFException($"Can't load shortcuts from {filename}!");
			}
		}

		private KVObject ToKVObject()
		{
			var kvList = new List<KVObject>();
			for (var i = 0; i < this.Shortcuts.Count; i++)
			{
				var shortcut = this.Shortcuts[i];
				kvList.Add(shortcut.ToKVObject(i));
			}

			return new KVObject("shortcuts", kvList);
		}

		public void Save(string filename)
		{
			using var fs = File.Open(filename, FileMode.Create);
			var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Binary);
			kv.Serialize(fs, this.ToKVObject());
		}
	}
}
