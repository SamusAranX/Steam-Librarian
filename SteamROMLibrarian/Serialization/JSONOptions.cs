using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SteamROMLibrarian.Serialization;

internal class JSON
{
	public static JsonSerializerOptions Options = new()
	{
		AllowTrailingCommas = true,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		ReadCommentHandling = JsonCommentHandling.Skip,
		WriteIndented = true,
	};
}

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
[JsonSerializable(typeof(GameLibrary))]
internal partial class JsonContext : JsonSerializerContext { }
