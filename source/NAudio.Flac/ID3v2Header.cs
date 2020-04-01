using System;
using System.IO;

namespace NAudio.Flac
{
	public class ID3v2Header
	{
		public const int HeaderLength = 10;

		public ID3Version Version
		{
			get;
			private set;
		}

		public byte[] RawVersion
		{
			get;
			private set;
		}

		public int DataLength
		{
			get;
			private set;
		}

		public ID3v2HeaderFlags Flags
		{
			get;
			private set;
		}

		public bool IsUnsync => (Flags & ID3v2HeaderFlags.Unsynchronisation) == ID3v2HeaderFlags.Unsynchronisation;

		public static ID3v2Header FromStream(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (!stream.CanRead)
			{
				throw new ArgumentException("stream not readable");
			}
			if (!stream.CanSeek)
			{
				throw new ArgumentException("stream not seekable");
			}
			new BinaryReader(stream);
			byte[] array = new byte[10];
			int num = stream.Read(array, 0, array.Length);
			if (num < 10)
			{
				throw new EndOfStreamException();
			}
			if (array[0] == 73 && array[1] == 68 && array[2] == 51)
			{
				ID3v2Header iD3v2Header = new ID3v2Header();
				iD3v2Header.Version = (ID3Version)array[3];
				iD3v2Header.RawVersion = new byte[2]
				{
					array[3],
					array[4]
				};
				iD3v2Header.Flags = (ID3v2HeaderFlags)array[5];
				iD3v2Header.DataLength = ID3Utils.ReadInt32(array, 6, sync: true);
				return iD3v2Header;
			}
			stream.Position -= num;
			return null;
		}

		private ID3v2Header()
		{
		}
	}
}
