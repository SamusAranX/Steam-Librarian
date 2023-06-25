using System.CommandLine;
using System.Text;

namespace SteamROMLibrarian;

internal class Program
{
	private static int Main(string[] args)
	{
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		Console.OutputEncoding = Encoding.UTF8;

		var root = new RootCommand();
		root.Description = "A tool to add non-Steam apps and games with custom artwork to your library. The root command will print this help message. Use the subcommands.";
		root.SetHandler(() => root.Invoke("-h"));

		#region Options

		var libraryPathOption = new Option<string>(
			"--library",
			"The path to the library JSON file."
		);
		libraryPathOption.IsRequired = true;
		libraryPathOption.AddAlias("-l");

		var userIDOption = new Option<string?>(
			"--user-id",
			description: "Disable automatic user ID detection and use this specific one.",
			getDefaultValue: () => null
		);
		userIDOption.AddAlias("-u");

		var overwriteOption = new Option<bool>(
			"--overwrite",
			description: "Overwrite library.json if it already exists.",
			getDefaultValue: () => false
		);

		var entryPathsOption = new Option<bool>(
			"--entry-paths",
			description: "Show entry file paths.",
			getDefaultValue: () => false
		);

		var fixLibraryOption = new Option<bool>(
			"--fix",
			description: "Automatically fix library image paths.",
			getDefaultValue: () => false
		);
		fixLibraryOption.AddAlias("-f");

		#endregion

		var prepareCommand = new Command("prepare", "Reads Steam shortcuts and prepares a library JSON file. You should only need to use this once.");
		prepareCommand.AddOption(userIDOption);
		prepareCommand.AddOption(libraryPathOption);
		prepareCommand.AddOption(overwriteOption);

		var checkCommand = new Command("check", "Reads the library JSON file and checks it for errors.");
		checkCommand.AddOption(libraryPathOption);
		checkCommand.AddOption(entryPathsOption);
		checkCommand.AddOption(fixLibraryOption);

		var writeCommand = new Command("write", "Writes library JSON to Steam library.");
		writeCommand.AddOption(userIDOption);
		writeCommand.AddOption(libraryPathOption);

		var writeExampleCommand = new Command("write-example", "Outputs an example library JSON file.");

		var resetCollectionsCommand = new Command("reset", "Runs steam://resetcollections.");

		prepareCommand.SetHandler(Librarian.PrepareLibrary, userIDOption, libraryPathOption, overwriteOption);
		checkCommand.SetHandler(Librarian.CheckLibrary, libraryPathOption, entryPathsOption, fixLibraryOption);
		writeCommand.SetHandler(Librarian.WriteLibrary, userIDOption, libraryPathOption);
		writeExampleCommand.SetHandler(Librarian.WriteExampleLibrary);
		resetCollectionsCommand.SetHandler(Librarian.ResetCollections);

		root.Add(prepareCommand);
		root.Add(checkCommand);
		root.Add(writeCommand);
		root.Add(writeExampleCommand);
		root.Add(resetCollectionsCommand);

		return root.Invoke(args);
	}
}
