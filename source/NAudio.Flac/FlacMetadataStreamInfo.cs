using System.IO;
using System.Text;

namespace NAudio.Flac
{
	public class FlacMetadataStreamInfo : FlacMetadata
	{
		public short MinBlockSize
		{
			get;
			private set;
		}

		public short MaxBlockSize
		{
			get;
			private set;
		}

		public uint MaxFrameSize
		{
			get;
			private set;
		}

		public uint MinFrameSize
		{
			get;
			private set;
		}

		public int SampleRate
		{
			get;
			private set;
		}

		public int Channels
		{
			get;
			private set;
		}

		public int BitsPerSample
		{
			get;
			private set;
		}

		public long TotalSamples
		{
			get;
			private set;
		}

		public string MD5
		{
			get;
			private set;
		}

		public unsafe FlacMetadataStreamInfo(Stream stream, int length, bool lastBlock)
			: base(FlacMetaDataType.StreamInfo, lastBlock, length)
		{
			BinaryReader binaryReader = new BinaryReader(stream, Encoding.ASCII);
			try
			{
				MinBlockSize = binaryReader.ReadInt16();
				MaxBlockSize = binaryReader.ReadInt16();
			}
			catch (IOException innerexception)
			{
				throw new FlacException(innerexception, FlacLayer.Metadata);
			}
			int num = 14;
			byte[] array = binaryReader.ReadBytes(num);
			if (array.Length != num)
			{
				throw new FlacException(new EndOfStreamException("Could not read StreamInfo-content"), FlacLayer.Metadata);
			}
			fixed (byte* buffer = array)
			{
				FlacBitReader flacBitReader = new FlacBitReader(buffer, 0);
				MinFrameSize = flacBitReader.ReadBits(24);
				MaxFrameSize = flacBitReader.ReadBits(24);
				SampleRate = (int)flacBitReader.ReadBits(20);
				Channels = (int)(1 + flacBitReader.ReadBits(3));
				BitsPerSample = (int)(1 + flacBitReader.ReadBits(5));
				TotalSamples = (long)flacBitReader.ReadBits64(36);
				MD5 = new string(binaryReader.ReadChars(16));
			}
		}
	}
}
