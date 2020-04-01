using System;
using System.IO;

namespace NAudio.Flac
{
	public class ID3v2ExtendedHeader
	{
		public int HeaderLength
		{
			get;
			private set;
		}

		public int PaddingSize
		{
			get;
			private set;
		}

		public ID3v2ExtendedHeaderFlags Flags
		{
			get;
			private set;
		}

		public byte[] CRC
		{
			get;
			private set;
		}

		public ID3v2TagSizeRestriction TagSizeRestriction
		{
			get;
			private set;
		}

		public ID3v2TextEncodingRestriction TextEncodingRestriction
		{
			get;
			private set;
		}

		public ID3v2TextFieldSizeRestriction TextFieldSizeRestriction
		{
			get;
			private set;
		}

		public ID3v2ImageEncodingRestriction ImageEncodingRestriction
		{
			get;
			private set;
		}

		public ID3v2ImageSizeRestriction ImageSizeRestriction
		{
			get;
			private set;
		}

		public ID3Version Version
		{
			get;
			private set;
		}

		public ID3v2ExtendedHeader(Stream stream, ID3Version version)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (!stream.CanRead)
			{
				throw new ArgumentException("stream not readable");
			}
			Version = version;
			switch (version)
			{
			case ID3Version.ID3v2_3:
				Parse3(stream);
				break;
			case ID3Version.ID3v2_4:
				Parse4(stream);
				break;
			default:
				throw new ID3Exception("Invalid version in extendedheader");
			}
		}

		private void Parse3(Stream stream)
		{
			HeaderLength = ID3Utils.ReadInt32(stream, sync: false);
			byte[] array = ID3Utils.Read(stream, 2);
			PaddingSize = ID3Utils.ReadInt32(stream, sync: false);
			if ((array[0] & 0x7F) != 0 || array[1] != 0)
			{
				throw new ID3Exception("Invalid ExtendedHeaderflags");
			}
			if ((array[0] & 0x80) != 0)
			{
				Flags |= ID3v2ExtendedHeaderFlags.CrcPresent;
				CRC = ID3Utils.Read(stream, 4);
			}
		}

		private void Parse4(Stream stream)
		{
			HeaderLength = ID3Utils.ReadInt32(stream, sync: false);
			byte[] array = ID3Utils.Read(stream, 1);
			Flags = (ID3v2ExtendedHeaderFlags)array[0];
			if ((Flags & ID3v2ExtendedHeaderFlags.CrcPresent) == ID3v2ExtendedHeaderFlags.CrcPresent)
			{
				CRC = ID3Utils.Read(stream, 5);
			}
			if ((Flags & ID3v2ExtendedHeaderFlags.Restrict) == ID3v2ExtendedHeaderFlags.Restrict)
			{
				TagSizeRestriction = (ID3v2TagSizeRestriction)(array[0] & 0xC0);
				TextEncodingRestriction = (ID3v2TextEncodingRestriction)(array[0] & 0x20);
				TextFieldSizeRestriction = (ID3v2TextFieldSizeRestriction)(array[0] & 0x18);
				ImageEncodingRestriction = (ID3v2ImageEncodingRestriction)(array[0] & 4);
				ImageSizeRestriction = (ID3v2ImageSizeRestriction)(array[0] & 3);
			}
			if ((array[0] & 0x8F) != 0)
			{
				throw new ID3Exception("Invalid Extendedheaderflags");
			}
		}
	}
}
