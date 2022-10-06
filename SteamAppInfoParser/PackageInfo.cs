using System.Collections.ObjectModel;
using ValveKeyValue;

namespace VDFLib
{
	class PackageInfo
	{
		private const uint MAGIC = 0x06_56_55_28;
		private const uint MAGIC27 = 0x06_56_55_27;

		public EUniverse Universe { get; set; }

		public List<Package> Packages { get; set; } = new List<Package>();

		/// <summary>
		/// Opens and reads the given filename.
		/// </summary>
		/// <param name="filename">The file to open and read.</param>
		public void Read(string filename)
		{
			using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			this.Read(fs);
		}

		/// <summary>
		/// Reads the given <see cref="Stream"/>.
		/// </summary>
		/// <param name="input">The input <see cref="Stream"/> to read from.</param>
		public void Read(Stream input)
		{
			using var reader = new BinaryReader(input);
			var magic = reader.ReadUInt32();

			if (magic != MAGIC && magic != MAGIC27)
			{
				throw new InvalidDataException($"Unknown magic header: {magic}");
			}

			this.Universe = (EUniverse)reader.ReadUInt32();

			var deserializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Binary);

			do
			{
				var subid = reader.ReadUInt32();

				if (subid == 0xFFFFFFFF)
				{
					break;
				}

				var package = new Package
				{
					SubID = subid,
					Hash = new ReadOnlyCollection<byte>(reader.ReadBytes(20)),
					ChangeNumber = reader.ReadUInt32(),
				};

				if (magic != MAGIC27)
				{
					package.Token = reader.ReadUInt64();
				}

				package.Data = deserializer.Deserialize(input);

				this.Packages.Add(package);
			}
			while (true);
		}
	}
}
