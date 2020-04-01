namespace NAudio.Flac
{
	internal class CRC8 : CRCBase<byte>
	{
		private static CRC8 _instance;

		public static CRC8 Instance => _instance ?? (_instance = new CRC8());

		public CRC8()
		{
			CalcTable(8);
		}

		public override byte CalcCheckSum(byte[] buffer, int offset, int count)
		{
			int num = 0;
			for (int i = offset; i < offset + count; i++)
			{
				num = crc_table[num ^ buffer[i]];
			}
			return (byte)num;
		}

		public unsafe byte CalcCheckSum(byte* buffer, int offset, int count)
		{
			int num = 0;
			for (int i = offset; i < offset + count; i++)
			{
				num = crc_table[num ^ buffer[i]];
			}
			return (byte)num;
		}
	}
}
