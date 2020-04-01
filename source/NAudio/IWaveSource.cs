using System;

namespace NAudio
{
	public interface IWaveSource : IWaveStream, IDisposable
	{
		int Read(byte[] buffer, int offset, int count);
	}
}
