namespace NAudio.Flac
{
	public class Popularimeter : Frame
	{
		public string UserEmail
		{
			get;
			private set;
		}

		public byte Rating
		{
			get;
			private set;
		}

		public long PlayedCounter
		{
			get;
			private set;
		}

		public Popularimeter(FrameHeader header)
			: base(header)
		{
		}

		protected override void Decode(byte[] content)
		{
			int num = 0;
			UserEmail = ID3Utils.ReadString(content, 0, -1, ID3Utils.Iso88591, out int read);
			num += read;
			Rating = content[num];
			num++;
			if (num < content.Length)
			{
				int num2 = 0;
				for (int i = num; i < content.Length; i++)
				{
					PlayedCounter |= (uint)(content[i] << num2);
					num2 += 8;
				}
			}
		}
	}
}
