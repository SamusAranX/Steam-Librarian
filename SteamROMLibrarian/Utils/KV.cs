using System.Text;

namespace SteamROMLibrarian.Utils;

internal class KV
{
	private static readonly Encoding ISO88592 = Encoding.GetEncoding("ISO-8859-2");

	private static string GetIntString(byte[] bytes, bool bigEndian = false)
	{
		if (bigEndian && BitConverter.IsLittleEndian)
			Array.Reverse(bytes);

		return ISO88592.GetString(bytes);
	}

	public static string GetIntString(int value, bool bigEndian = false)
	{
		return GetIntString(BitConverter.GetBytes(value), bigEndian);
	}

	public static string GetIntString(uint value, bool bigEndian = false)
	{
		return GetIntString(BitConverter.GetBytes(value), bigEndian);
	}
}
