using System.Text;

namespace SteamROMLibrarian.Utils
{
	internal class KV
	{
		private static readonly Encoding ISO88591 = Encoding.GetEncoding("ISO-8859-1");
		private static readonly Encoding ISO88592 = Encoding.GetEncoding("ISO-8859-2");
		private static readonly Encoding WINDOWS1252 = Encoding.GetEncoding("Windows-1252");

		private static string GetIntString(byte[] bytes, bool bigEndian = false)
		{
			if (bigEndian && BitConverter.IsLittleEndian)
				Array.Reverse(bytes);

			return ISO88592.GetString(bytes);
		}

		//public static string GetIntString(short value, bool bigEndian = false) => GetIntString(BitConverter.GetBytes(value), bigEndian);
		//public static string GetIntString(ushort value, bool bigEndian = false) => GetIntString(BitConverter.GetBytes(value), bigEndian);
		public static string GetIntString(int value, bool bigEndian = false) => GetIntString(BitConverter.GetBytes(value), bigEndian);

		public static string GetIntString(uint value, bool bigEndian = false) => GetIntString(BitConverter.GetBytes(value), bigEndian);

		//public static string GetIntString(long value, bool bigEndian = false) => GetIntString(BitConverter.GetBytes(value), bigEndian);
		//public static string GetIntString(ulong value, bool bigEndian = false) => GetIntString(BitConverter.GetBytes(value), bigEndian);
	}
}
