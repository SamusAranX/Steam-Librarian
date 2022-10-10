using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SteamROMLibrarian.Utils;

namespace SteamROMLibrarian.Serialization
{
	[Serializable]
	internal class ROMLauncher
	{
		public string Executable { get; set; }
		public string Arguments { get; set; }

		public ROMLauncher(string executable, string arguments)
		{
			this.Executable = executable;
			this.Arguments = arguments;
		}
	}

	[Serializable]
	internal class Category
	{
		public string DefaultLauncher { get; set; }
		public List<ROMEntry> Entries { get; set; }

		public Category(string defaultLauncher, List<ROMEntry> entries)
		{
			this.DefaultLauncher = defaultLauncher;
			this.Entries = entries;
		}
	}

	[Serializable]
	internal class SteamMetadata
	{
		public string? AppID { get; set; }

		public string? BPMAppID { get; set; }

		[JsonIgnore]
		public int LastPlayTimeUnix { get; set; }

		public DateTime LastPlayTime
		{
			get => new DateTime(1970, 1, 1).ToLocalTime().AddSeconds(this.LastPlayTimeUnix);
			set => this.LastPlayTimeUnix = (int)((DateTimeOffset)value).ToUnixTimeSeconds();
		}
	}

	[Serializable]
	internal class ROMEntry : IJsonOnDeserialized
	{
		public string? Launcher { get; set; }
		public string Name { get; set; }
		public string? Path { get; set; }
		public bool VR { get; set; }
		public string? Grid { get; set; }
		public string? Poster { get; set; }
		public string? Hero { get; set; }
		public string? Logo { get; set; }
		public string? Icon { get; set; }

		[JsonPropertyName("steamMetadataDoNotEdit")]
		public SteamMetadata Metadata { get; set; } = new();

		public void OnDeserialized()
		{
			this.GenerateAppID();
		}

		[JsonConstructor]
		public ROMEntry(string name, string? path = null)
		{
			this.Name = name;
			this.Path = path;

			// even though this constructor says [JSONConstructor] on it,
			// the separate OnDeserialized() is necessary because when deserializing
			// from JSON, this constructor doesn't seem to be called at all.
			// this makes no sense. thanks, microsoft

			this.GenerateAppID();
		}

		public ROMEntry(string name, string path, string launcher) : this(name, path)
		{
			this.Launcher = launcher;
		}

		private void GenerateAppID()
		{
			var appID = new AppID(this.Path ?? "", this.Name);
			this.Metadata.AppID ??= appID.ShortcutID;
			this.Metadata.BPMAppID ??= appID.LegacyID;
		}
	}

	[Serializable]
	internal class ShortcutPointer
	{
		public string AppID { get; set; }
		public string AppName { get; set; }

		public ShortcutPointer(string appID, string appName)
		{
			this.AppID = appID;
			this.AppName = appName;
		}
	}

	[Serializable]
	internal class GameLibrary
	{
		// keys are referenced in ROMEntry.Launcher
		public Dictionary<string, ROMLauncher> Launchers { get; set; }

		// keys are category names
		public Dictionary<string, Category> Categories { get; set; }

		// list of serialized Shortcut entries for preexisting non-ROM non-Steam entries (e.g. Fallout 4 launched via F4SE)
		public List<ShortcutPointer> PreexistingShortcuts { get; set; }

		public GameLibrary()
		{
			this.Launchers = new Dictionary<string, ROMLauncher>();
			this.Categories = new Dictionary<string, Category>();
			this.PreexistingShortcuts = new List<ShortcutPointer>();
		}

		public static GameLibrary Load(string libraryPath)
		{
			using var fs = File.OpenRead(libraryPath);
			var library = JsonSerializer.Deserialize<GameLibrary>(fs, JSON.Options);

			if (library == null)
				throw new InvalidLibraryException($"Can't load library from ${libraryPath}!");

			return library;
		}

		public string ToJSON()
		{
			var jsonString = JsonSerializer.Serialize(this, JSON.Options);
			using var sr = new StringReader(jsonString);
			var sb = new StringBuilder();

			while (sr.ReadLine() is { } line)
			{
				// filter out lines with empty string values
				if (line.EndsWith(": \"\","))
					continue;

				sb.AppendLine(line);
			}

			return sb.ToString();
		}

		public void Save(string libraryPath)
		{
			using var fs = File.Open(libraryPath, FileMode.Create);
			using var sw = new StreamWriter(fs);
			sw.Write(this.ToJSON());
		}

		public ROMEntry? GetByID(string appID)
		{
			foreach (var (_, category) in this.Categories)
			{
				foreach (var entry in category.Entries)
				{
					if (entry.Metadata.AppID == appID)
						return entry;
				}
			}

			return null;
		}

		public bool Contains(string appID)
		{
			return this.GetByID(appID) != null;
		}

		public bool ContainsPreexisting(string appID)
		{
			foreach (var sp in this.PreexistingShortcuts)
			{
				if (sp.AppID == appID)
					return true;
			}

			return false;
		}

		public static GameLibrary ExampleLibrary =>
			new()
			{
				Launchers = new Dictionary<string, ROMLauncher>
				{
					{ "dreamcast", new ROMLauncher("/usr/bin/flatpak", "run org.flycast.Flycast") },
					{ "mgba", new ROMLauncher("/usr/bin/flatpak", "run io.mgba.mGBA -f") },
					{ "sameboy", new ROMLauncher("/usr/bin/flatpak", "run io.github.sameboy.SameBoy -f") },
					{ "dolphin", new ROMLauncher("/usr/bin/flatpak", "run org.DolphinEmu.dolphin-emu -b -e") },
				},
				Categories = new Dictionary<string, Category>
				{
					{
						"Dreamcast", new Category("dreamcast", new List<ROMEntry>
							{
								new("Dreamcast BIOS", "Dreamcast BIOS")
								{
									Metadata =
									{
										LastPlayTimeUnix = 1665162720, // 2022-10-07T18:12:00+02:00
									},
								},
							}
						)
					},
					{
						"GB/GBA", new Category("mgba", new List<ROMEntry>
							{
								new("Test Cartridge", "/run/media/mmcblk0p1/roms/gba/Jayro's Test Cartridge v1.15.gba"),
								new("LSDj", "/run/media/mmcblk0p1/roms/gb/lsdj9_2_L.gb", "sameboy"),
							}
						)
					},
					{
						"GameCube/Wii", new Category("dolphin", new List<ROMEntry>
							{
								new("Controller Test", "/run/media/mmcblk0p1/roms/wii/240pTestSuite/boot.dol")
								{
									Grid = "/run/media/mmcblk0p1/images/240pTestSuite_grid.png",
									Poster = "/run/media/mmcblk0p1/images/240pTestSuite_poster.png",
									Hero = "/run/media/mmcblk0p1/images/240pTestSuite_hero.png",
									Logo = "/run/media/mmcblk0p1/images/240pTestSuite_logo.png",
									Icon = "/run/media/mmcblk0p1/images/240pTestSuite_icon.png",
								},
							}
						)
					},
				},
				PreexistingShortcuts = new List<ShortcutPointer>
				{
					new("1234567890", "Example"),
				},
			};
	}
}
