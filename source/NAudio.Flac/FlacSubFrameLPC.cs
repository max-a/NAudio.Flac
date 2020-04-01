using System;

namespace NAudio.Flac
{
	public sealed class FlacSubFrameLPC : FlacSubFrameBase
	{
		private readonly int[] _warmup;

		private readonly int[] _qlpCoeffs;

		private readonly int _lpcShiftNeeded;

		private readonly int _qlpCoeffPrecision;

		public int QLPCoeffPrecision => _qlpCoeffPrecision;

		public int LPCShiftNeeded => _lpcShiftNeeded;

		public int[] QLPCoeffs => _qlpCoeffs;

		public int[] Warmup => _warmup;

		public FlacResidual Residual
		{
			get;
			private set;
		}

		public unsafe FlacSubFrameLPC(FlacBitReader reader, FlacFrameHeader header, FlacSubFrameData data, int bps, int order)
			: base(header)
		{
			_warmup = new int[32];
			for (int i = 0; i < order; i++)
			{
				_warmup[i] = (data.ResidualBuffer[i] = reader.ReadBitsSigned(bps));
			}
			int num = (int)reader.ReadBits(4);
			if (num == 15)
			{
				return;
			}
			_qlpCoeffPrecision = num + 1;
			int num2 = reader.ReadBitsSigned(5);
			if (num2 < 0)
			{
				throw new Exception("negative shift");
			}
			_lpcShiftNeeded = num2;
			_qlpCoeffs = new int[32];
			for (int j = 0; j < order; j++)
			{
				_qlpCoeffs[j] = reader.ReadBitsSigned(_qlpCoeffPrecision);
			}
			Residual = new FlacResidual(reader, header, data, order);
			for (int k = 0; k < order; k++)
			{
				data.DestBuffer[k] = data.ResidualBuffer[k];
			}
			int num3 = order;
			int num4 = 0;
			while ((num3 >>= 1) != 0)
			{
				num4++;
			}
			if (bps + _qlpCoeffPrecision + num4 <= 32)
			{
				if (bps <= 16 && _qlpCoeffPrecision <= 16)
				{
					RestoreLPCSignal(data.ResidualBuffer + order, data.DestBuffer + order, header.BlockSize - order, order);
				}
				else
				{
					RestoreLPCSignal(data.ResidualBuffer + order, data.DestBuffer + order, header.BlockSize - order, order);
				}
			}
			else
			{
				RestoreLPCSignalWide(data.ResidualBuffer + order, data.DestBuffer + order, header.BlockSize - order, order);
			}
		}

		private unsafe void Restore(int* residual, int* dest, int length, int predictorOrder, int destOffset)
		{
			for (int i = 0; i < length; i++)
			{
				int num = 0;
				for (int j = 0; j < predictorOrder; j++)
				{
					num += _qlpCoeffs[j] * dest[destOffset + i - j - 1];
				}
				dest[destOffset + i] = residual[i] + (num >> _lpcShiftNeeded);
			}
		}

		private unsafe void RestoreLPCSignal(int* residual, int* destination, int length, int order)
		{
			int num = 0;
			int* ptr = residual;
			int* ptr2 = destination;
			for (int i = 0; i < length; i++)
			{
				num = 0;
				int* ptr3 = ptr2;
				for (int j = 0; j < order; j++)
				{
					num += _qlpCoeffs[j] * *(--ptr3);
				}
				int* intPtr = ptr2;
				ptr2 = intPtr + 1;
				int* intPtr2 = ptr;
				ptr = intPtr2 + 1;
				*intPtr = *intPtr2 + (num >> _lpcShiftNeeded);
			}
		}

		private unsafe void RestoreLPCSignalWide(int* residual, int* destination, int length, int order)
		{
			long num = 0L;
			int* ptr = residual;
			int* ptr2 = destination;
			for (int i = 0; i < length; i++)
			{
				num = 0L;
				int* ptr3 = ptr2;
				for (int j = 0; j < order; j++)
				{
					num += (long)_qlpCoeffs[j] * (long)(*(--ptr3));
				}
				int* intPtr = ptr2;
				ptr2 = intPtr + 1;
				int* intPtr2 = ptr;
				ptr = intPtr2 + 1;
				*intPtr = *intPtr2 + (int)(num >> _lpcShiftNeeded);
			}
		}
	}
}
