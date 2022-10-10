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
			name: "--library",
			description: "The path to the library JSON file."
		);
		libraryPathOption.IsRequired = true;
		libraryPathOption.AddAlias("-l");
		root.AddGlobalOption(libraryPathOption);

		var userIDOption = new Option<string?>(
			name: "--user-id",
			description: "Disable automatic user ID detection and use this specific one.",
			getDefaultValue: () => null
		);
		userIDOption.AddAlias("-u");

		var overwriteOption = new Option<bool>(
			name: "--overwrite",
			description: "Overwrite library.json if it already exists.",
			getDefaultValue: () => false
		);

		#endregion

		var prepareCommand = new Command("prepare", "Reads Steam shortcuts and prepares a library JSON file. You should only need to use this once.");
		prepareCommand.AddOption(userIDOption);
		prepareCommand.AddOption(overwriteOption);
		root.Add(prepareCommand);

		var writeCommand = new Command("write", "Writes library JSON to Steam library.");
		writeCommand.AddOption(userIDOption);
		root.Add(writeCommand);

		var writeExampleCommand = new Command("write-example", "Outputs an example library JSON file.");
		root.Add(writeExampleCommand);

		prepareCommand.SetHandler(Librarian.PrepareLibrary, userIDOption, libraryPathOption, overwriteOption);
		writeCommand.SetHandler(Librarian.WriteLibrary, userIDOption, libraryPathOption);
		writeExampleCommand.SetHandler(Librarian.WriteExampleLibrary, libraryPathOption);

		return root.Invoke(args);
	}
}
