using System.CommandLine;
using System.Text;
using SteamKit2.Internal;
using SteamROMLibrarian.Utils;

namespace SteamROMLibrarian;

internal class Program
{
	private static int Main(string[] args)
	{
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

		//for (uint i = 0; i < uint.MaxValue; i++)
		//{
		//	var str = KV.GetIntString(i);
		//	Console.WriteLine(str);
		//}


		//return 0;

		Console.OutputEncoding = Encoding.UTF8;

		var root = new RootCommand();
		root.Description = "A tool to add non-Steam apps and games with custom artwork to your library. Use the read or write commands.";
		root.SetHandler(() => root.Invoke("-h"));

		#region Options

		var libraryPathOption = new Option<string>(
			name: "--library",
			description: "The path to the library JSON file.",
			getDefaultValue: () => "library.json"
		);
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

		var writeExampleCommand = new Command("write-example", "Writes example library JSON to the directory given in --library.");
		root.Add(writeExampleCommand);

		prepareCommand.SetHandler(Librarian.PrepareLibrary, userIDOption, libraryPathOption, overwriteOption);
		writeCommand.SetHandler(Librarian.WriteLibrary, userIDOption, libraryPathOption);
		writeExampleCommand.SetHandler(Librarian.WriteExampleLibrary, libraryPathOption);

		return root.Invoke(args);
	}
}
