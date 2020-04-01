namespace NAudio.Flac
{
	public class FlacResidual
	{
		public FlacEntropyCoding CodingMethod
		{
			get;
			private set;
		}

		public int RiceOrder
		{
			get;
			private set;
		}

		internal FlacPartitionedRice Rice
		{
			get;
			private set;
		}

		public FlacResidual(FlacBitReader reader, FlacFrameHeader header, FlacSubFrameData data, int order)
		{
			FlacEntropyCoding flacEntropyCoding = (FlacEntropyCoding)reader.ReadBits(2);
			if (flacEntropyCoding == FlacEntropyCoding.PartitionedRice || flacEntropyCoding == FlacEntropyCoding.PartitionedRice2)
			{
				if (!new FlacPartitionedRice((int)reader.ReadBits(4), flacEntropyCoding, data.Content).ProcessResidual(reader, header, data, order))
				{
					throw new FlacException("Decoding Flac Residual failed.", FlacLayer.SubFrame);
				}
				return;
			}
			throw new FlacException("Not supported RICE-Coding-Method. Stream unparseable!", FlacLayer.SubFrame);
		}
	}
}
