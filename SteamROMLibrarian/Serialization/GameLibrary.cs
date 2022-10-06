using System.Text.Json;
using System.Text.Json.Serialization;

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
	internal class ROMEntry: IJsonOnDeserialized
	{
		public string? AppID { get; set; }
		public string? Launcher { get; set; }
		public string Name { get; set; }
		public string? Path { get; set; }
		public string? Grid { get; set; }
		public string? Poster { get; set; }
		public string? Hero { get; set; }
		public string? Logo { get; set; }
		public string? Icon { get; set; }
		
		public void OnDeserialized()
		{
			this.RegenerateAppID();
		}

		[JsonConstructor]
		public ROMEntry(string name, string? path = null)
		{
			this.Name = name;
			this.Path = path;

			this.RegenerateAppID();
		}

		public ROMEntry(string name, string path, string launcher) : this(name, path)
		{
			this.Launcher = launcher;
		}

		private void RegenerateAppID()
		{
			this.AppID = Utils.AppID.GenerateGridAppID(this.Path ?? "", this.Name);
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
		public List<Shortcut> LeaveUntouched { get; set; }
		
		public GameLibrary()
		{
			this.Launchers = new Dictionary<string, ROMLauncher>();
			this.Categories = new Dictionary<string, Category>();
			this.LeaveUntouched = new List<Shortcut>();
		}

		public static GameLibrary Deserialize(string libraryFile)
		{
			using var fs = File.OpenRead(libraryFile);
			return JsonSerializer.Deserialize<GameLibrary>(fs, JSON.Options);
		}

		public void Serialize(string libraryFile)
		{
			using var fs = File.OpenWrite(libraryFile);
			JsonSerializer.Serialize(fs, this, JSON.Options);
		}

		public static GameLibrary ExampleLibrary =>
			new()
			{
				Launchers = new Dictionary<string, ROMLauncher>
				{
					{ "dreamcast", new ROMLauncher("/usr/bin/flatpak", "run org.flycast.Flycast") },
					{ "dolphin", new ROMLauncher("/usr/bin/flatpak", "run org.DolphinEmu.dolphin-emu -b -e") },
				},
				Categories = new Dictionary<string, Category>
				{
					{
						"Dreamcast", new Category("dreamcast", new List<ROMEntry>
							{
								new("Dreamcast BIOS", "Dreamcast BIOS"),
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
				LeaveUntouched = new List<Shortcut>
				{
					new()
					{
						AppID = 1234567890,
						AppName = "Fallout 4 (F4SE)",
						Exe = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Fallout 4\\f4se_loader.exe",
						StartDir = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Fallout 4\\",
					},
				},
			};
	}
}
