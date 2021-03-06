namespace PRGReaderLibrary
{
    using System.Linq;
    using System.Collections.Generic;

    public class TablePoint : Version, IBinaryObject
    {
        public string Name { get; set; }
        public List<TableValue> Values { get; set; } = new List<TableValue>();

        public TablePoint(string name = "", List<TableValue> values = null,
            FileVersion version = FileVersion.Current)
            : base(version)
        {
            Name = name;
            Values = values ?? Values;
        }

        #region Binary data

        public static int GetCount(FileVersion version = FileVersion.Current)
        {
            switch (version)
            {
                case FileVersion.Current:
                    return 5;

                default:
                    throw new FileVersionNotImplementedException(version);
            }
        }

        public static int GetSize(FileVersion version = FileVersion.Current)
        {
            switch (version)
            {
                case FileVersion.Current:
                    return 105;

                default:
                    throw new FileVersionNotImplementedException(version);
            }
        }

        /// <summary>
        /// FileVersion.Current - Need 105 bytes
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="version"></param>
        public TablePoint(byte[] bytes, int offset = 0,
            FileVersion version = FileVersion.Current)
            : base(version)
        {
            switch (FileVersion)
            {
                case FileVersion.Current:
                    Name = bytes.GetString(ref offset, 9).ClearBinarySymvols();
                    for (int i = 0; i < 16; ++i)
                    {
                        Values.Add(new TableValue(bytes.ToBytes(ref offset, 6), 0, FileVersion));
                    }
                    break;

                default:
                    throw new FileVersionNotImplementedException(FileVersion);
            }

            CheckOffset(offset, GetSize(FileVersion));
        }

        /// <summary>
        /// FileVersion.Current - 105 bytes
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            var bytes = new List<byte>();

            switch (FileVersion)
            {
                case FileVersion.Current:
                    bytes.AddRange(Name.ToBytes(9));
                    for (int i = 0; i < 16; ++i)
                    {
                        var value = Values.ElementAtOrDefault(i) ?? new TableValue();
                        value.FileVersion = FileVersion;
                        bytes.AddRange(value.ToBytes());
                    }
                    break;

                default:
                    throw new FileVersionNotImplementedException(FileVersion);
            }

            CheckSize(bytes.Count, GetSize(FileVersion));

            return bytes.ToArray();
        }

        #endregion
    }
}