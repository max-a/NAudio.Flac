using System;
using System.IO;

namespace NAudio.Flac
{
	public class ID3v2Footer
	{
		public const int FooterLength = 10;

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

		public long DataLength
		{
			get;
			private set;
		}

		public ID3v2HeaderFlags Flags
		{
			get;
			private set;
		}

		public static ID3v2Footer FromStream(Stream stream)
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
				ID3v2Footer iD3v2Footer = new ID3v2Footer();
				iD3v2Footer.Version = (ID3Version)array[3];
				iD3v2Footer.RawVersion = new byte[2]
				{
					array[3],
					array[4]
				};
				iD3v2Footer.Flags = (ID3v2HeaderFlags)array[5];
				iD3v2Footer.DataLength = ID3Utils.ReadInt32(array, 6, sync: true);
				return iD3v2Footer;
			}
			stream.Position -= num;
			return null;
		}

		private ID3v2Footer()
		{
		}
	}
}
