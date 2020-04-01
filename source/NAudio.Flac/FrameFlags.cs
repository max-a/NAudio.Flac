using System;

namespace NAudio.Flac
{
	[Flags]
	public enum FrameFlags
	{
		None = 0x0,
		ReadOnly = 0x1,
		PreserveFileAltered = 0x2,
		PreserveTagAltered = 0x4,
		Compressed = 0x8,
		Encrypted = 0xA,
		GroupIdentified = 0xC,
		UnsyncApplied = 0xE,
		DataLengthIndicatorPresent = 0x10
	}
}
