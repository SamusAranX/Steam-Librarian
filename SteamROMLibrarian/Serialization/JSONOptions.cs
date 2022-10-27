using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace SteamROMLibrarian.Serialization
{
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
}
