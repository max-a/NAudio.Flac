using System;

namespace NAudio.Flac
{
	internal abstract class CRCBase<T> where T : struct
	{
		protected readonly int tableSize = 256;

		protected ushort[] crc_table;

		protected void CalcTable(int bits)
		{
			if (bits != 8 && bits != 16)
			{
				throw new ArgumentOutOfRangeException("bits has to be 8 or 16");
			}
			int num = (bits == 8) ? 7 : 32773;
			int num2 = (bits == 8) ? 255 : 65535;
			crc_table = new ushort[tableSize];
			int num3 = (ushort)(num + (1 << bits));
			for (int i = 0; i < crc_table.Length; i++)
			{
				int num4 = i;
				for (int j = 0; j < bits; j++)
				{
					num4 = (((num4 & (1 << bits - 1)) == 0) ? (num4 << 1) : ((num4 << 1) ^ num3));
				}
				crc_table[i] = (ushort)(num4 & num2);
			}
		}

		public abstract T CalcCheckSum(byte[] buffer, int offset, int count);
	}
}
