namespace NAudio.Flac
{
	public class FlacBitReader : BitReader
	{
		internal static readonly byte[] UnaryTable = new byte[256]
		{
			8,
			7,
			6,
			6,
			5,
			5,
			5,
			5,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			4,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			3,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0
		};

		public FlacBitReader(byte[] buffer, int offset)
			: base(buffer, offset)
		{
		}

		public unsafe FlacBitReader(byte* buffer, int offset)
			: base(buffer, offset)
		{
		}

		public uint ReadUnary()
		{
			uint num = 0u;
			uint num2;
			for (num2 = base.Cache >> 24; num2 == 0; num2 = base.Cache >> 24)
			{
				SeekBits(8);
				num += 8;
			}
			num += UnaryTable[num2];
			SeekBits((int)((num & 7) + 1));
			return num;
		}

		public int ReadUnarySigned()
		{
			uint num = ReadUnary();
			return (int)((num >> 1) ^ (int)(0 - (num & 1)));
		}

		public bool ReadUTF8_64(out ulong result)
		{
			uint num = ReadBits(8);
			int num3;
			ulong num2;
			if ((num & 0x80) == 0)
			{
				num2 = num;
				num3 = 0;
			}
			else if ((num & 0xC0) != 0 && (num & 0x20) == 0)
			{
				num2 = (num & 0x1F);
				num3 = 1;
			}
			else if ((num & 0xE0) != 0 && (num & 0x10) == 0)
			{
				num2 = (num & 0xF);
				num3 = 2;
			}
			else if ((num & 0xF0) != 0 && (num & 8) == 0)
			{
				num2 = (num & 7);
				num3 = 3;
			}
			else if ((num & 0xF8) != 0 && (num & 4) == 0)
			{
				num2 = (num & 3);
				num3 = 4;
			}
			else if ((num & 0xFC) != 0 && (num & 2) == 0)
			{
				num2 = (num & 1);
				num3 = 5;
			}
			else
			{
				if ((num & 0xFE) == 0 || (num & 1) != 0)
				{
					result = ulong.MaxValue;
					return false;
				}
				num2 = 0uL;
				num3 = 6;
			}
			while (num3 != 0)
			{
				num = ReadBits(8);
				if ((num & 0xC0) != 128)
				{
					result = ulong.MaxValue;
					return false;
				}
				num2 <<= 6;
				num2 |= (num & 0x3F);
				num3--;
			}
			result = num2;
			return true;
		}

		public bool ReadUTF8_32(out uint result)
		{
			uint num = ReadBits(8);
			int num3;
			uint num2;
			if ((num & 0x80) == 0)
			{
				num2 = num;
				num3 = 0;
			}
			else if ((num & 0xC0) != 0 && (num & 0x20) == 0)
			{
				num2 = (num & 0x1F);
				num3 = 1;
			}
			else if ((num & 0xE0) != 0 && (num & 0x10) == 0)
			{
				num2 = (num & 0xF);
				num3 = 2;
			}
			else if ((num & 0xF0) != 0 && (num & 8) == 0)
			{
				num2 = (num & 7);
				num3 = 3;
			}
			else if ((num & 0xF8) != 0 && (num & 4) == 0)
			{
				num2 = (num & 3);
				num3 = 4;
			}
			else
			{
				if ((num & 0xFC) == 0 || (num & 2) != 0)
				{
					result = uint.MaxValue;
					return false;
				}
				num2 = (num & 1);
				num3 = 5;
			}
			while (num3 != 0)
			{
				num = ReadBits(8);
				if ((num & 0xC0) != 128)
				{
					result = uint.MaxValue;
					return false;
				}
				num2 <<= 6;
				num2 |= (num & 0x3F);
				num3--;
			}
			result = num2;
			return true;
		}
	}
}
