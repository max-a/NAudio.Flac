namespace NAudio.Flac
{
	public sealed class FlacSubFrameConstant : FlacSubFrameBase
	{
		public int Value
		{
			get;
			private set;
		}

		public unsafe FlacSubFrameConstant(FlacBitReader reader, FlacFrameHeader header, FlacSubFrameData data, int bps)
			: base(header)
		{
			Value = (int)reader.ReadBits(bps);
			for (int i = 0; i < header.BlockSize; i++)
			{
				*data.DestBuffer = Value;
			}
		}
	}
}
