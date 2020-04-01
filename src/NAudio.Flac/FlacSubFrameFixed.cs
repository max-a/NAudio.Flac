namespace NAudio.Flac
{
	public sealed class FlacSubFrameFixed : FlacSubFrameBase
	{
		public FlacResidual Residual
		{
			get;
			private set;
		}

		public unsafe FlacSubFrameFixed(FlacBitReader reader, FlacFrameHeader header, FlacSubFrameData data, int bps, int order)
			: base(header)
		{
			for (int i = 0; i < order; i++)
			{
				data.ResidualBuffer[i] = (data.DestBuffer[i] = reader.ReadBitsSigned(bps));
			}
			Residual = new FlacResidual(reader, header, data, order);
			RestoreSignal(data, header.BlockSize - order, order);
		}

		private unsafe bool RestoreSignal(FlacSubFrameData subframeData, int length, int predictorOrder)
		{
			int* ptr = subframeData.ResidualBuffer + predictorOrder;
			int* ptr2 = subframeData.DestBuffer + predictorOrder;
			switch (predictorOrder)
			{
			case 0:
			{
				for (int j = 0; j < length; j++)
				{
					int* intPtr = ptr2;
					ptr2 = intPtr + 1;
					int* intPtr2 = ptr;
					ptr = intPtr2 + 1;
					*intPtr = *intPtr2;
				}
				break;
			}
			case 1:
			{
				int num = ptr2[-1];
				for (int l = 0; l < length; l++)
				{
					int num2 = num;
					int* intPtr3 = ptr;
					ptr = intPtr3 + 1;
					num = num2 + *intPtr3;
					int* intPtr4 = ptr2;
					ptr2 = intPtr4 + 1;
					*intPtr4 = num;
				}
				break;
			}
			case 2:
			{
				int num3 = ptr2[-2];
				int num = ptr2[-1];
				for (int m = 0; m < length; m++)
				{
					int* intPtr5 = ptr2;
					ptr2 = intPtr5 + 1;
					int num4 = num << 1;
					int* intPtr6 = ptr;
					ptr = intPtr6 + 1;
					int num5 = *intPtr5 = num4 + *intPtr6 - num3;
					num3 = num;
					num = num5;
				}
				break;
			}
			case 3:
			{
				for (int k = 0; k < length; k++)
				{
					*ptr2 = *ptr + ((ptr2[-1] - ptr2[-2] << 1) + (ptr2[-1] - ptr2[-2])) + ptr2[-3];
					ptr2++;
					ptr++;
				}
				break;
			}
			case 4:
			{
				for (int i = 0; i < length; i++)
				{
					*ptr2 = *ptr + (ptr2[-1] + ptr2[-3] << 2) - ((ptr2[-2] << 2) + (ptr2[-2] << 1)) - ptr2[-4];
					ptr2++;
					ptr++;
				}
				break;
			}
			default:
				return false;
			}
			return true;
		}
	}
}
