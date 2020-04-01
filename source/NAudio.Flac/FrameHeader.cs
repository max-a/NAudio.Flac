using System;
using System.IO;
using System.Linq;

namespace NAudio.Flac
{
	public class FrameHeader
	{
		private int _uncompressedSize;

		private int _dataLengthIndicator;

		private byte _encryptionMethod;

		public string FrameID
		{
			get;
			private set;
		}

		public int FrameSize
		{
			get;
			private set;
		}

		public FrameFlags Flags
		{
			get;
			private set;
		}

		public byte GroupIdentifier
		{
			get;
			private set;
		}

		public FrameIDFactory2.ID3v2FrameEntry GetFrameInformation()
		{
			return FrameIDFactory2.Frames.Where((FrameIDFactory2.ID3v2FrameEntry x) => x.ID3v4ID == FrameID || x.ID3v3ID == FrameID || x.ID3v2ID == FrameID).First();
		}

		public FrameHeader(Stream stream, ID3Version version)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (!stream.CanRead)
			{
				throw new ArgumentException("stream is not readable");
			}
			switch (version)
			{
			case ID3Version.ID3v2_2:
				Parse2(stream);
				break;
			case ID3Version.ID3v2_3:
				Parse3(stream);
				break;
			case ID3Version.ID3v2_4:
				Parse4(stream);
				break;
			default:
				throw new ID3Exception("Invalid ID3Version in Frameheader");
			}
		}

		private void Parse2(Stream stream)
		{
			byte[] array = ID3Utils.Read(stream, 6);
			FrameID = ID3Utils.ReadString(array, 0, 3, ID3Utils.Iso88591);
			FrameSize = ID3Utils.ReadInt32(array, 3, sync: false, 3);
			Flags = FrameFlags.None;
		}

		private void Parse3(Stream stream)
		{
			byte[] array = ID3Utils.Read(stream, 10);
			FrameID = ID3Utils.ReadString(array, 0, 4, ID3Utils.Iso88591);
			FrameSize = ID3Utils.ReadInt32(array, 4, sync: false, 4);
			byte[] obj = new byte[2]
			{
				array[8],
				array[9]
			};
			if ((obj[0] & 0x80) == 0)
			{
				Flags |= FrameFlags.PreserveTagAltered;
			}
			if ((obj[0] & 0x40) == 0)
			{
				Flags |= FrameFlags.PreserveFileAltered;
			}
			if ((obj[0] & 0x20) != 0)
			{
				Flags |= FrameFlags.ReadOnly;
			}
			if ((obj[1] & 0x80) != 0)
			{
				Flags |= FrameFlags.Compressed;
				_uncompressedSize = ID3Utils.ReadInt32(stream, sync: false);
				FrameSize -= 4;
			}
			if ((obj[1] & 0x40) != 0)
			{
				Flags |= FrameFlags.Encrypted;
				_encryptionMethod = ID3Utils.Read(stream, 1)[0];
				FrameSize--;
			}
			if ((obj[1] & 0x20) != 0)
			{
				Flags |= FrameFlags.GroupIdentified;
				GroupIdentifier = ID3Utils.Read(stream, 1)[0];
				FrameSize--;
			}
		}

		private void Parse4(Stream stream)
		{
			byte[] array = ID3Utils.Read(stream, 10);
			FrameID = ID3Utils.ReadString(array, 0, 4, ID3Utils.Iso88591);
			FrameSize = ID3Utils.ReadInt32(array, 4, sync: true, 4);
			byte[] obj = new byte[2]
			{
				array[8],
				array[9]
			};
			if ((obj[0] & 0x40) == 0)
			{
				Flags |= FrameFlags.PreserveTagAltered;
			}
			if ((obj[0] & 0x20) == 0)
			{
				Flags |= FrameFlags.PreserveFileAltered;
			}
			if ((obj[0] & 0x10) != 0)
			{
				Flags |= FrameFlags.ReadOnly;
			}
			if ((obj[1] & 0x40) != 0)
			{
				Flags |= FrameFlags.GroupIdentified;
				GroupIdentifier = ID3Utils.Read(stream, 1)[0];
				FrameSize--;
			}
			if ((obj[1] & 8) != 0)
			{
				Flags |= FrameFlags.Compressed;
			}
			if ((obj[1] & 4) != 0)
			{
				Flags |= FrameFlags.Encrypted;
				_encryptionMethod = ID3Utils.Read(stream, 1)[0];
				FrameSize--;
			}
			if ((obj[1] & 2) != 0)
			{
				Flags |= FrameFlags.UnsyncApplied;
			}
			if ((obj[1] & 1) != 0)
			{
				Flags |= FrameFlags.DataLengthIndicatorPresent;
				_dataLengthIndicator = ID3Utils.ReadInt32(stream, sync: true);
				FrameSize -= 4;
			}
		}
	}
}
