using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace NAudio.Flac
{
	internal sealed class FlacPreScan
	{
		private const int BufferSize = 50000;

		private readonly Stream _stream;

		private bool _isRunning;

		public List<FlacFrameInformation> Frames
		{
			get;
			private set;
		}

		public long TotalLength
		{
			get;
			private set;
		}

		public long TotalSamples
		{
			get;
			private set;
		}

		public event EventHandler<FlacPreScanFinishedEventArgs> ScanFinished;

		public FlacPreScan(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (!stream.CanRead)
			{
				throw new ArgumentException("stream is not readable");
			}
			_stream = stream;
		}

		public void ScanStream(FlacMetadataStreamInfo streamInfo, FlacPreScanMethodMode mode)
		{
			long position = _stream.Position;
			StartScan(streamInfo, mode);
			_stream.Position = position;
			long num = 0L;
			long num2 = 0L;
			foreach (FlacFrameInformation frame in Frames)
			{
				num += frame.Header.BlockSize * frame.Header.BitsPerSample * frame.Header.Channels;
				num2 += frame.Header.BlockSize;
			}
			TotalLength = num;
			TotalSamples = num2;
		}

		private void StartScan(FlacMetadataStreamInfo streamInfo, FlacPreScanMethodMode method)
		{
			if (_isRunning)
			{
				throw new Exception("Scan is already running.");
			}
			_isRunning = true;
			if (method == FlacPreScanMethodMode.Async)
			{
				ThreadPool.QueueUserWorkItem(delegate
				{
					Frames = RunScan(streamInfo);
					_isRunning = false;
				});
				return;
			}
			Frames = RunScan(streamInfo);
			_isRunning = false;
		}

		private List<FlacFrameInformation> RunScan(FlacMetadataStreamInfo streamInfo)
		{
			List<FlacFrameInformation> list = ScanThisShit(streamInfo);
			RaiseScanFinished(list);
			return list;
		}

		private void RaiseScanFinished(List<FlacFrameInformation> frames)
		{
			if (this.ScanFinished != null)
			{
				this.ScanFinished(this, new FlacPreScanFinishedEventArgs(frames));
			}
		}

		private unsafe List<FlacFrameInformation> ScanThisShit(FlacMetadataStreamInfo streamInfo)
		{
			Stream stream = _stream;
			byte[] array = new byte[50000];
			int num = 0;
			stream.Position = 4L;
			FlacMetadata.ReadAllMetadataFromStream(stream);
			List<FlacFrameInformation> list = new List<FlacFrameInformation>();
			FlacFrameInformation item = default(FlacFrameInformation);
			item.IsFirstFrame = true;
			FlacFrameHeader flacFrameHeader = null;
			while (true)
			{
				num = stream.Read(array, 0, array.Length);
				if (num <= 16)
				{
					break;
				}
				fixed (byte* ptr = array)
				{
					byte* buffer = ptr;
					while (ptr + num - 16 > buffer)
					{
						if ((*(buffer++) & 0xFF) == 255 && (*buffer & 0xF8) == 248)
						{
							byte* ptr2 = buffer;
							buffer--;
							FlacFrameHeader header = null;
							if (IsFrame(ref buffer, streamInfo, flacFrameHeader, out header))
							{
								FlacFrameHeader flacFrameHeader2 = header;
								if (item.IsFirstFrame)
								{
									flacFrameHeader = flacFrameHeader2;
									item.IsFirstFrame = false;
								}
								if (flacFrameHeader.CompareTo(flacFrameHeader2))
								{
									item.StreamOffset = stream.Position - num + (ptr2 - 1 - ptr);
									item.Header = flacFrameHeader2;
									list.Add(item);
									item.SampleOffset += flacFrameHeader2.BlockSize;
								}
								else
								{
									buffer = ptr2;
								}
							}
							else
							{
								buffer = ptr2;
							}
						}
					}
				}
				stream.Position -= 16L;
			}
			return list;
		}

		private unsafe bool IsFrame(ref byte* buffer, FlacMetadataStreamInfo streamInfo, FlacFrameHeader baseHeader, out FlacFrameHeader header)
		{
			header = new FlacFrameHeader(ref buffer, streamInfo, doCrc: true, logError: false);
			return !header.HasError;
		}
	}
}
