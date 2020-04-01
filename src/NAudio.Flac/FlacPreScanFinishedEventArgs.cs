using System;
using System.Collections.Generic;

namespace NAudio.Flac
{
	public class FlacPreScanFinishedEventArgs : EventArgs
	{
		public List<FlacFrameInformation> Frames
		{
			get;
			private set;
		}

		public FlacPreScanFinishedEventArgs(List<FlacFrameInformation> frames)
		{
			Frames = frames;
		}
	}
}
