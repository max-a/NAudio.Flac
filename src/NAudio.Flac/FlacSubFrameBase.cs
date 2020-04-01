namespace NAudio.Flac
{
	public class FlacSubFrameBase
	{
		public int WastedBits
		{
			get;
			protected set;
		}

		public FlacFrameHeader Header
		{
			get;
			protected set;
		}

		public unsafe static FlacSubFrameBase GetSubFrame(FlacBitReader reader, FlacSubFrameData data, FlacFrameHeader header, int bps)
		{
			int num = 0;
			int num2 = 0;
			uint num3 = reader.ReadBits(8);
			bool flag = (num3 & 1) != 0;
			num3 &= 0xFE;
			if (flag)
			{
				num = (int)(reader.ReadUnary() + 1);
				bps -= num;
			}
			if ((num3 & 0x80) != 0)
			{
				return null;
			}
			if ((num3 > 2 && num3 < 16) || (num3 > 24 && num3 < 64))
			{
				return null;
			}
			FlacSubFrameBase flacSubFrameBase;
			switch (num3)
			{
			case 0u:
				flacSubFrameBase = new FlacSubFrameConstant(reader, header, data, bps);
				break;
			case 2u:
				flacSubFrameBase = new FlacSubFrameVerbatim(reader, header, data, bps);
				break;
			case 16u:
			case 17u:
			case 18u:
			case 19u:
			case 20u:
			case 21u:
			case 22u:
			case 23u:
			case 24u:
				num2 = (int)((num3 >> 1) & 7);
				flacSubFrameBase = new FlacSubFrameFixed(reader, header, data, bps, num2);
				break;
			default:
				if (num3 >= 64)
				{
					num2 = (int)(((num3 >> 1) & 0x1F) + 1);
					flacSubFrameBase = new FlacSubFrameLPC(reader, header, data, bps, num2);
					break;
				}
				return null;
			}
			if (flag)
			{
				int* ptr = data.DestBuffer;
				for (int i = 0; i < header.BlockSize; i++)
				{
					int* intPtr = ptr;
					ptr = intPtr + 1;
					*intPtr <<= num;
				}
			}
			flacSubFrameBase.WastedBits = num;
			return flacSubFrameBase;
		}

		protected FlacSubFrameBase(FlacFrameHeader header)
		{
			Header = header;
		}
	}
}
