using System;

namespace NAudio.Flac
{
	public class FlacPartitionedRice
	{
		public int PartitionOrder
		{
			get;
			private set;
		}

		public FlacPartitionedRiceContent Content
		{
			get;
			private set;
		}

		public FlacEntropyCoding CodingMethod
		{
			get;
			private set;
		}

		public FlacPartitionedRice(int partitionOrder, FlacEntropyCoding codingMethod, FlacPartitionedRiceContent content)
		{
			PartitionOrder = partitionOrder;
			CodingMethod = codingMethod;
			Content = content;
		}

		public unsafe bool ProcessResidual(FlacBitReader reader, FlacFrameHeader header, FlacSubFrameData data, int order)
		{
			data.Content.UpdateSize(PartitionOrder);
			int partitionOrder = PartitionOrder;
			FlacEntropyCoding codingMethod = CodingMethod;
			int num = header.BlockSize >> partitionOrder;
			int val = num - order;
			int num2 = (int)(4 + codingMethod);
			int num3 = order;
			int* ptr = data.ResidualBuffer + num3;
			int num4 = 1 << partitionOrder;
			for (int i = 0; i < num4; i++)
			{
				if (i == 1)
				{
					val = num;
				}
				int num5 = Math.Min(val, header.BlockSize - num3);
				int num6 = Content.Parameters[i] = (int)reader.ReadBits(num2);
				if (num6 == (1 << num2) - 1)
				{
					num6 = (int)reader.ReadBits(5);
					for (int num7 = num5; num7 > 0; num7--)
					{
						*ptr = reader.ReadBitsSigned(num6);
					}
				}
				else
				{
					ReadFlacRiceBlock(reader, num5, num6, ptr);
					ptr += num5;
				}
				num3 += num5;
			}
			return true;
		}

		private unsafe void ReadFlacRiceBlock(FlacBitReader reader, int nvals, int riceParameter, int* ptrDest)
		{
			fixed (byte* ptr = FlacBitReader.UnaryTable)
			{
				uint num = (uint)((1 << riceParameter) - 1);
				if (riceParameter == 0)
				{
					for (int i = 0; i < nvals; i++)
					{
						int* intPtr = ptrDest;
						ptrDest = intPtr + 1;
						*intPtr = reader.ReadUnarySigned();
					}
				}
				else
				{
					for (int j = 0; j < nvals; j++)
					{
						uint num2 = ptr[reader.Cache >> 24];
						uint num3 = num2;
						while (num2 == 8)
						{
							reader.SeekBits(8);
							num2 = ptr[reader.Cache >> 24];
							num3 += num2;
						}
						uint num4 = 0u;
						if (riceParameter <= 16)
						{
							int num5 = riceParameter + (int)num2 + 1;
							num4 = ((num3 << riceParameter) | ((reader.Cache >> 32 - num5) & num));
							reader.SeekBits(num5);
						}
						else
						{
							reader.SeekBits((int)((num3 & 7) + 1));
							num4 = ((num3 << riceParameter) | (reader.Cache >> 32 - riceParameter));
							reader.SeekBits(riceParameter);
						}
						int* intPtr2 = ptrDest;
						ptrDest = intPtr2 + 1;
						*intPtr2 = (int)((num4 >> 1) ^ (int)(0 - (num4 & 1)));
					}
				}
			}
		}
	}
}
