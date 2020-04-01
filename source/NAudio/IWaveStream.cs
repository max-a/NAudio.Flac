using NAudio.Wave;
using System;

namespace NAudio
{
	public interface IWaveStream : IDisposable
	{
		bool CanSeek
		{
			get;
		}

		WaveFormat WaveFormat
		{
			get;
		}

		long Position
		{
			get;
			set;
		}

		long Length
		{
			get;
		}
	}
}
