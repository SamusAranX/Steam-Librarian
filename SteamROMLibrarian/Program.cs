using System.CommandLine;

namespace SteamROMLibrarian;

internal class Program
{
	private static int Main(string[] args)
	{
		var root = new RootCommand();
		root.Description = "A tool to add non-Steam apps and games with custom artwork to your library. Use the read or write commands.";
		root.SetHandler(() => root.Invoke("-h"));

		var userIDOption = new Option<string?>(
			name: "--user-id",
			description: "Disable automatic user ID detection and use this specific one.",
			getDefaultValue: () => null
		);
		userIDOption.AddAlias("-u");
		root.AddGlobalOption(userIDOption);

		var libraryPathOption = new Option<string>(
			name: "--library",
			description: "The path to the library JSON file.",
			getDefaultValue: () => "library.json"
		);
		libraryPathOption.AddAlias("-l");
		root.AddGlobalOption(libraryPathOption);

		var readCommand = new Command("read", "Reads Steam library. Use this to regenerate the library JSON file.");
		root.Add(readCommand);

		var writeCommand = new Command("write", "Writes library JSON to Steam library.");
		root.Add(writeCommand);

		readCommand.SetHandler(Librarian.ReadLibrary, userIDOption, libraryPathOption);
		readCommand.SetHandler(Librarian.WriteLibrary, userIDOption, libraryPathOption);

		return root.Invoke(args);
	}
}
