using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace NAudio.Flac
{
	[DebuggerDisplay("Type:{MetaDataType}   LastBlock:{IsLastMetaBlock}   Length:{Length} bytes")]
	public class FlacMetadata
	{
		public FlacMetaDataType MetaDataType
		{
			get;
			private set;
		}

		public bool IsLastMetaBlock
		{
			get;
			private set;
		}

		public int Length
		{
			get;
			private set;
		}

		public unsafe static FlacMetadata FromStream(Stream stream)
		{
			bool flag = false;
			FlacMetaDataType flacMetaDataType = FlacMetaDataType.Undef;
			int num = 0;
			byte[] array = new byte[4];
			if (stream.Read(array, 0, 4) <= 0)
			{
				throw new FlacException(new EndOfStreamException("Could not read metadata"), FlacLayer.Metadata);
			}
			fixed (byte* buffer = array)
			{
				FlacBitReader flacBitReader = new FlacBitReader(buffer, 0);
				flag = (flacBitReader.ReadBits(1) == 1);
				flacMetaDataType = (FlacMetaDataType)flacBitReader.ReadBits(7);
				num = (int)flacBitReader.ReadBits(24);
			}
			long position = stream.Position;
			FlacMetadata result;
			switch (flacMetaDataType)
			{
			default:
				return null;
			case FlacMetaDataType.StreamInfo:
				result = new FlacMetadataStreamInfo(stream, num, flag);
				break;
			case FlacMetaDataType.Seektable:
				result = new FlacMetadataSeekTable(stream, num, flag);
				break;
			case FlacMetaDataType.Padding:
			case FlacMetaDataType.Application:
			case FlacMetaDataType.VorbisComment:
			case FlacMetaDataType.CueSheet:
			case FlacMetaDataType.Picture:
				result = new FlacMetadata(flacMetaDataType, flag, num);
				break;
			}
			stream.Seek(num - (stream.Position - position), SeekOrigin.Current);
			return result;
		}

		public static List<FlacMetadata> ReadAllMetadataFromStream(Stream stream)
		{
			List<FlacMetadata> list = new List<FlacMetadata>();
			FlacMetadata flacMetadata;
			do
			{
				flacMetadata = FromStream(stream);
				if (flacMetadata != null)
				{
					list.Add(flacMetadata);
				}
			}
			while (flacMetadata != null && !flacMetadata.IsLastMetaBlock);
			return list;
		}

		protected FlacMetadata(FlacMetaDataType type, bool lastBlock, int length)
		{
			MetaDataType = type;
			IsLastMetaBlock = lastBlock;
			Length = length;
		}
	}
}
