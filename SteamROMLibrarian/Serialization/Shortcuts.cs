using System.Collections;
using System.Text.Json;
using SteamKit2;
using SteamROMLibrarian.Utils;

namespace SteamROMLibrarian.Serialization
{
	[Serializable]
	internal class Shortcut
	{
		public uint AppID { get; set; }
		public string? AppName { get; set; }
		public string? Exe { get; set; }
		public string? StartDir { get; set; }
		public string? Icon { get; set; }
		public string? ShortcutPath { get; set; }
		public string? LaunchOptions { get; set; }
		public bool IsHidden { get; set; }
		public bool AllowDesktopConfig { get; set; } = true;
		public bool AllowOverlay { get; set; } = true;
		public bool OpenVR { get; set; }
		public bool Devkit { get; set; }
		public uint DevkitGameID { get; set; }
		public uint DevkitOverrideAppID { get; set; }
		public DateTime? LastPlayTime { get; set; }
		public string? FlatpakAppID { get; set; }
		public List<string>? Tags { get; set; }

		public Shortcut()
		{
			this.Tags = new List<string>();
		}

		public Shortcut(KeyValue kv)
		{
			this.AppID = (uint)kv["appid"].AsInteger();
			this.AppName = kv["AppName"].AsString();
			this.Exe = kv["Exe"].AsString();
			this.StartDir = kv["StartDir"].AsString();
			this.Icon = kv["icon"].AsString();

			if (kv["ShortcutPath"].AsString() != "")
				this.ShortcutPath = kv["ShortcutPath"].AsString();

			if (kv["LaunchOptions"].AsString() != "")
				this.LaunchOptions = kv["LaunchOptions"].AsString();

			this.IsHidden = kv["IsHidden"].AsBoolean();
			this.AllowDesktopConfig = kv["AllowDesktopConfig"].AsBoolean();
			this.AllowOverlay = kv["AllowOverlay"].AsBoolean();
			this.OpenVR = kv["OpenVR"].AsBoolean();
			this.Devkit = kv["Devkit"].AsBoolean();
			this.DevkitGameID = kv["DevkitGameID"].AsUnsignedInteger();
			this.DevkitOverrideAppID = kv["DevkitOverrideAppID"].AsUnsignedInteger();

			if (kv["LastPlayTime"].AsUnsignedInteger() != 0)
				this.LastPlayTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(kv["LastPlayTime"].AsUnsignedInteger());

			if (kv["FlatpakAppID"].AsString() != "")
				this.FlatpakAppID = kv["FlatpakAppID"].AsString();

			if (kv["tags"].Children.Count > 0)
			{
				var tempTags = new string[kv["tags"].Children.Count];
				foreach (var kvTag in kv["tags"].Children)
				{
					// if any of these are null, we might as well crash
					var index = uint.Parse(kvTag.Name!);
					tempTags[index] = kvTag.AsString()!;
				}

				this.Tags = tempTags.ToList();
			}
		}

		public KeyValue ToKeyValue(int index)
		{
			var kv = new KeyValue(index.ToString());

			// TODO: figure out how to encode integers
			// TODO: make keys other than appid use the Value = syntax

			kv["appid"] = new KeyValue { Value = KV.GetIntString(this.AppID) };
			kv["AppName"].Value = this.AppName;
			kv["Exe"].Value = this.Exe;
			kv["StartDir"].Value = this.StartDir;
			kv["icon"].Value = this.Icon;
			kv["ShortcutPath"].Value = this.ShortcutPath;
			kv["LaunchOptions"].Value = this.LaunchOptions;
			kv["IsHidden"].Value = KV.GetIntString(this.IsHidden ? 1 : 0);
			kv["AllowDesktopConfig"].Value = KV.GetIntString(this.AllowDesktopConfig ? 1 : 0);
			kv["AllowOverlay"].Value = KV.GetIntString(this.AllowOverlay ? 1 : 0);
			kv["OpenVR"].Value = KV.GetIntString(this.OpenVR ? 1 : 0);
			kv["Devkit"].Value = KV.GetIntString(this.Devkit ? 1 : 0);
			kv["DevkitGameID"].Value = KV.GetIntString(this.DevkitGameID);
			kv["DevkitOverrideAppID"].Value = KV.GetIntString(this.DevkitOverrideAppID);

			if (this.LastPlayTime != null)
				kv["LastPlayTime"].Value = KV.GetIntString((uint)((DateTimeOffset)this.LastPlayTime).ToUnixTimeSeconds());
			else
				kv["LastPlayTime"].Value = KV.GetIntString(0);

			kv["FlatpakAppID"].Value = this.FlatpakAppID;

			if (this.Tags != null)
			{
				kv["tags"] = new KeyValue();
				for (var i = 0; i < this.Tags.Count; i++)
				{
					var kvTag = new KeyValue(i.ToString(), this.Tags[i]);
					kv["tags"].Children.Add(kvTag);
				}
			}

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
		public List<Shortcut> Shortcuts { get; set; }

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

		public ShortcutsVDF(string filename)
		{
			if (!KeyValue.TryLoadAsBinary(filename, out var shortcuts))
			{
				throw new InvalidVDFException("Couldn't load shortcuts.vdf");
			}

			Console.WriteLine(shortcuts);

			var tempShortcuts = new Shortcut[shortcuts.Children.Count];
			foreach (var kv in shortcuts.Children)
			{
				var index = uint.Parse(kv.Name!); // kv.Name should never be null in this case. Might as well crash here.
				tempShortcuts[index] = new Shortcut(kv);
			}

			this.Shortcuts = tempShortcuts.ToList();
		}

		public void Save(string filename)
		{
			var kv = new KeyValue("shortcuts");
			for (int i = 0; i < this.Shortcuts.Count; i++)
			{
				var shortcut = this.Shortcuts[i];
				kv.Children.Add(shortcut.ToKeyValue(i));
			}

			kv.SaveToFile(filename, true);
		}

		public string ToJSONString()
		{
			return JsonSerializer.Serialize(this, JSON.Options);
		}
	}
}
