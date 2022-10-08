using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SteamROMLibrarian.Serialization
{
	internal static class Extensions
	{
		public static bool IsQuoted(this string s)
		{
			return s.StartsWith("\"") && s.EndsWith("\"");
		}

		public static string ToQuotedString(this string s)
		{
			return $"\"{s}\"";
		}
	}
}
