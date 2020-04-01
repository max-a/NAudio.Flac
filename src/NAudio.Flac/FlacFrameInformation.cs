using System.Diagnostics;

namespace NAudio.Flac
{
	[DebuggerDisplay("StreamOffset: {StreamOffset}")]
	public struct FlacFrameInformation
	{
		public FlacFrameHeader Header
		{
			get;
			set;
		}

		public bool IsFirstFrame
		{
			get;
			set;
		}

		public long StreamOffset
		{
			get;
			set;
		}

		public long SampleOffset
		{
			get;
			set;
		}
	}
}
