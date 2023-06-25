using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SteamROMLibrarian.Utils;
using IOPath = System.IO.Path;

namespace SteamROMLibrarian.Serialization;

[Serializable]
internal class ROMLauncher
{
	public ROMLauncher(string executable, string arguments)
	{
		this.Executable = executable;
		this.Arguments = arguments;
	}

	public string Executable { get; set; }
	public string Arguments { get; set; }
}

[Serializable]
internal class Category
{
	public Category(string? defaultLauncher, List<ROMEntry> entries)
	{
		this.DefaultLauncher = defaultLauncher;
		this.Entries = entries;
	}

	[JsonConstructor]
	public Category(string? defaultLauncher, string? rootDirectory, List<ROMEntry> entries)
	{
		this.DefaultLauncher = defaultLauncher;
		this.RootDirectory = rootDirectory;
		this.Entries = entries;
	}

	public string? DefaultLauncher { get; set; }
	public string? RootDirectory { get; set; }
	public List<ROMEntry> Entries { get; set; }
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
	private AppID _appID;

	[JsonConstructor]
#pragma warning disable CS8618
	public ROMEntry(string name, string? path = null)
#pragma warning restore CS8618
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

	public string? Launcher { get; set; }
	public string Name { get; set; }
	public bool NoRoot { get; set; }
	public string? Command { get; set; }
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

	private void GenerateAppID()
	{
		this._appID = new AppID(this.Path ?? "", this.Name);
		this.Metadata.AppID ??= this._appID.ShortcutID;
		this.Metadata.BPMAppID ??= this._appID.LegacyID;
	}

	public string? SetImagePath(ImageType type, string? path)
	{
		return type switch
		{
			ImageType.Grid => this.Grid = path,
			ImageType.Poster => this.Poster = path,
			ImageType.Hero => this.Hero = path,
			ImageType.Logo => this.Logo = path,
			ImageType.Icon => this.Icon = path,
			ImageType.BigPictureGrid => this.Grid = path,
			_ => throw new ArgumentException("Invalid image type"),
		};
	}

	public string? GetImagePath(ImageType type)
	{
		return type switch
		{
			ImageType.Grid => this.Grid,
			ImageType.Poster => this.Poster,
			ImageType.Hero => this.Hero,
			ImageType.Logo => this.Logo,
			ImageType.Icon => this.Icon,
			ImageType.BigPictureGrid => this.Grid,
			_ => throw new ArgumentException("Invalid image type"),
		};
	}

	public string? GetSteamImageName(ImageType type)
	{
		var path = this.GetImagePath(type);
		if (path == null)
			return null;

		var appIDStr = this._appID.ShortcutID;
		var suffix = "";

		switch (type)
		{
			case ImageType.Poster:
				suffix = "p";
				break;
			case ImageType.Hero:
				suffix = "_hero";
				break;
			case ImageType.Logo:
				suffix = "_logo";
				break;
			case ImageType.Icon:
				suffix = "_icon";
				break;
			case ImageType.BigPictureGrid:
				appIDStr = this._appID.LegacyID;
				break;
		}

		var ext = IOPath.GetExtension(path).TrimStart('.');
		return $"{appIDStr}{suffix}.{ext}";
	}

	// if noRoot:
	// Launcher?.Executable + Launcher?.Arguments + Command? + Path?
	// else:
	// Launcher?.Executable + Launcher?.Arguments + Command? + (rootDirectory + Path)?

	public string Executable(string? rootDirectory, ROMLauncher? launcher)
	{
		var argsList = new List<string>();

		if (launcher != null)
		{
			argsList.Add($"\"{launcher.Executable}\"");
			if (launcher.Arguments.Trim() != "")
				argsList.Add(launcher.Arguments);
		}

		if (!string.IsNullOrWhiteSpace(this.Command))
			argsList.Add(this.Command);

		if (!this.NoRoot)
		{
			if (rootDirectory != null && this.Path != null)
				argsList.Add($"\"{IOPath.Combine(rootDirectory, this.Path)}\"");
		}
		else if (this.Path != null)
			argsList.Add($"\"{this.Path}\"");

		if (argsList.Count == 0)
			throw new ArgumentException($"Entry {this.Name} does not point to a valid executable");

		return string.Join(" ", argsList);
	}

	internal enum ImageType
	{
		Grid,
		Poster,
		Hero,
		Logo,
		Icon,
		BigPictureGrid,
	}
}

[Serializable]
internal class ShortcutPointer
{
	public ShortcutPointer(string appID, string appName)
	{
		this.AppID = appID;
		this.AppName = appName;
	}

	public string AppID { get; set; }
	public string AppName { get; set; }
}

[Serializable]
internal class GameLibrary
{
	public GameLibrary()
	{
		this.Launchers = new Dictionary<string, ROMLauncher>();
		this.Categories = new Dictionary<string, Category>();
		this.PreexistingShortcuts = new List<ShortcutPointer>();
	}

	// keys are referenced in ROMEntry.Launcher
	public Dictionary<string, ROMLauncher> Launchers { get; set; }

	// keys are category names
	public Dictionary<string, Category> Categories { get; set; }

	// list of serialized Shortcut entries for preexisting non-ROM non-Steam entries (e.g. Fallout 4 launched via F4SE)
	public List<ShortcutPointer> PreexistingShortcuts { get; set; }

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
					"GB/GBA", new Category("mgba", "/run/media/mmcblk0p1/roms", new List<ROMEntry>
						{
							new("Test Cartridge", "gba/Jayro's Test Cartridge v1.15.gba"),
							new("LSDj", "gb/lsdj9_2_L.gb", "sameboy"),
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
}
