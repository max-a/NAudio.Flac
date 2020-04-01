namespace NAudio.Flac
{
	public class FlacPartitionedRiceContent
	{
		public int[] Parameters;

		public int[] RawBits;

		private int _capByOrder = -1;

		public void UpdateSize(int po)
		{
			if (_capByOrder < po)
			{
				int num = 1 << po;
				Parameters = new int[num];
				RawBits = new int[num];
				_capByOrder = po;
			}
		}
	}
}
