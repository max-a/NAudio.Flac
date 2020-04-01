namespace NAudio.Flac
{
	public class FlacSubFrameData
	{
		public unsafe int* DestBuffer;

		public unsafe int* ResidualBuffer;

		private FlacPartitionedRiceContent _content;

		public FlacPartitionedRiceContent Content
		{
			get
			{
				return _content ?? (_content = new FlacPartitionedRiceContent());
			}
			set
			{
				_content = value;
			}
		}
	}
}
