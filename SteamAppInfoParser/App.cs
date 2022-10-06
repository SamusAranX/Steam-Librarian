﻿using System.Collections.ObjectModel;
using ValveKeyValue;

namespace VDFLib
{
	public class App
	{
		public uint AppID { get; set; }

		public uint Size { get; set; }

		public uint InfoState { get; set; }

		public DateTime LastUpdated { get; set; }

		public ulong Token { get; set; }

		public ReadOnlyCollection<byte> Hash { get; set; }

		public uint ChangeNumber { get; set; }

		public KVObject Data { get; set; }
	}
}
