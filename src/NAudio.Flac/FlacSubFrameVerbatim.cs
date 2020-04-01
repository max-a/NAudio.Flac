namespace NAudio.Flac
{
	public sealed class FlacSubFrameVerbatim : FlacSubFrameBase
	{
		public unsafe FlacSubFrameVerbatim(FlacBitReader reader, FlacFrameHeader header, FlacSubFrameData data, int bps)
			: base(header)
		{
			int* ptr = data.DestBuffer;
			int* ptr2 = data.ResidualBuffer;
			for (int i = 0; i < header.BlockSize; i++)
			{
				int num = (int)reader.ReadBits(bps);
				int* intPtr = ptr;
				ptr = intPtr + 1;
				*intPtr = num;
				int* intPtr2 = ptr2;
				ptr2 = intPtr2 + 1;
				*intPtr2 = num;
			}
		}
	}
}
