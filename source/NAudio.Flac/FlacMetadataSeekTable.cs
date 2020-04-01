using System.IO;

namespace NAudio.Flac
{
	public class FlacMetadataSeekTable : FlacMetadata
	{
		private readonly FlacSeekPoint[] seekPoints;

		public int EntryCount
		{
			get;
			private set;
		}

		public FlacSeekPoint[] SeekPoints
		{
			get;
			private set;
		}

		public FlacSeekPoint this[int index] => seekPoints[index];

		public FlacMetadataSeekTable(Stream stream, int length, bool lastBlock)
			: base(FlacMetaDataType.Seektable, lastBlock, length)
		{
			int num2 = EntryCount = length / 18;
			seekPoints = new FlacSeekPoint[num2];
			BinaryReader binaryReader = new BinaryReader(stream);
			try
			{
				for (int i = 0; i < num2; i++)
				{
					seekPoints[i] = new FlacSeekPoint(binaryReader.ReadInt64(), binaryReader.ReadInt64(), binaryReader.ReadInt16());
				}
			}
			catch (IOException innerexception)
			{
				throw new FlacException(innerexception, FlacLayer.Metadata);
			}
		}
	}
}
