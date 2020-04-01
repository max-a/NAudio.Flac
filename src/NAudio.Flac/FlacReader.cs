using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAudio.Flac
{
	public class FlacReader : WaveStream, IDisposable, ISampleProvider, IWaveProvider
	{
		private readonly Stream _stream;

		private readonly WaveFormat _waveFormat;

		private readonly FlacMetadataStreamInfo _streamInfo;

		private readonly FlacPreScan _scan;

		private readonly object _bufferLock = new object();

		private byte[] _overflowBuffer;

		private int _overflowCount;

		private int _overflowOffset;

		private int _frameIndex;

		private FlacFrame _frame;

		private long _position;

		public List<FlacMetadata> Metadata
		{
			get;
			protected set;
		}

		public override WaveFormat WaveFormat => _waveFormat;

		public override bool CanSeek => _scan != null;

		private FlacFrame Frame => _frame ?? (_frame = FlacFrame.FromStream(_stream, _streamInfo));

		public override long Position
		{
			get
			{
				if (!CanSeek)
				{
					return 0L;
				}
				lock (_bufferLock)
				{
					return _position;
				}
			}
			set
			{
				if (CanSeek)
				{
					lock (_bufferLock)
					{
						value = Math.Min(value, Length);
						value = ((value > 0) ? value : 0);
						int num = 0;
						while (true)
						{
							if (num >= _scan.Frames.Count)
							{
								return;
							}
							if (value / WaveFormat.BlockAlign <= _scan.Frames[num].SampleOffset)
							{
								break;
							}
							num++;
						}
						_stream.Position = _scan.Frames[num].StreamOffset;
						_frameIndex = num;
						if (_stream.Position >= _stream.Length)
						{
							throw new EndOfStreamException("Stream got EOF.");
						}
						_position = _scan.Frames[num].SampleOffset * WaveFormat.BlockAlign;
						_overflowCount = 0;
						_overflowOffset = 0;
					}
				}
			}
		}

		public override long Length => _scan.TotalSamples * WaveFormat.BlockAlign;

		public FlacReader(string fileName)
			: this(File.OpenRead(fileName))
		{
		}

		public FlacReader(Stream stream)
			: this(stream, FlacPreScanMethodMode.Default)
		{
		}

		public FlacReader(Stream stream, FlacPreScanMethodMode scanFlag)
			: this(stream, scanFlag, null)
		{
		}

		public FlacReader(Stream stream, FlacPreScanMethodMode scanFlag, Action<FlacPreScanFinishedEventArgs> onscanFinished)
		{
			if (stream == null)
			{
				throw new ArgumentNullException();
			}
			if (!stream.CanRead)
			{
				throw new ArgumentException("Stream is not readable.", "stream");
			}
			_stream = stream;
			ID3v2.SkipTag(stream);
			byte[] array = new byte[4];
			if (stream.Read(array, 0, array.Length) < array.Length)
			{
				throw new EndOfStreamException("Can not read \"fLaC\" sync.");
			}
			if (array[0] != 102 || array[1] != 76 || array[2] != 97 || array[3] != 67)
			{
				throw new FlacException("Invalid Flac-File. \"fLaC\" Sync not found.", FlacLayer.Top);
			}
			List<FlacMetadata> list2 = Metadata = FlacMetadata.ReadAllMetadataFromStream(stream);
			if (list2 == null || list2.Count <= 0)
			{
				throw new FlacException("No Metadata found.", FlacLayer.Metadata);
			}
			FlacMetadataStreamInfo flacMetadataStreamInfo = list2.First((FlacMetadata x) => x.MetaDataType == FlacMetaDataType.StreamInfo) as FlacMetadataStreamInfo;
			if (flacMetadataStreamInfo == null)
			{
				throw new FlacException("No StreamInfo-Metadata found.", FlacLayer.Metadata);
			}
			_streamInfo = flacMetadataStreamInfo;
			_waveFormat = new WaveFormat(flacMetadataStreamInfo.SampleRate, (short)flacMetadataStreamInfo.BitsPerSample, (short)flacMetadataStreamInfo.Channels);
			if (scanFlag != 0)
			{
				FlacPreScan flacPreScan = new FlacPreScan(stream);
				flacPreScan.ScanFinished += delegate(object s, FlacPreScanFinishedEventArgs e)
				{
					if (onscanFinished != null)
					{
						onscanFinished(e);
					}
				};
				flacPreScan.ScanStream(_streamInfo, scanFlag);
				_scan = flacPreScan;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int num = 0;
			int num2 = count;
			count = num2 - num2 % WaveFormat.BlockAlign;
			lock (_bufferLock)
			{
				num += GetOverflows(buffer, ref offset, count);
				while (num < count)
				{
					FlacFrame frame = Frame;
					if (frame == null)
					{
						return 0;
					}
					while (!frame.NextFrame())
					{
						if (CanSeek)
						{
							if (++_frameIndex >= _scan.Frames.Count)
							{
								return 0;
							}
							_stream.Position = _scan.Frames[_frameIndex].StreamOffset;
						}
					}
					_frameIndex++;
					int buffer2 = frame.GetBuffer(ref _overflowBuffer, 0);
					int num3 = Math.Min(count - num, buffer2);
					Array.Copy(_overflowBuffer, 0, buffer, offset, num3);
					num += num3;
					offset += num3;
					_overflowCount = ((buffer2 > num3) ? (buffer2 - num3) : 0);
					_overflowOffset = ((buffer2 > num3) ? num3 : 0);
				}
			}
			_position += num;
			return num;
		}

		private int GetOverflows(byte[] buffer, ref int offset, int count)
		{
			if (_overflowCount != 0 && _overflowBuffer != null && count > 0)
			{
				int num = Math.Min(count, _overflowCount);
				Array.Copy(_overflowBuffer, _overflowOffset, buffer, offset, num);
				_overflowCount -= num;
				_overflowOffset += num;
				offset += num;
				return num;
			}
			return 0;
		}

		public new void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected override void Dispose(bool disposing)
		{
			lock (_bufferLock)
			{
				if (_frame != null)
				{
					_frame.FreeBuffers();
					_frame = null;
				}
				if (_stream != null)
				{
					_stream.Dispose();
				}
			}
		}

		~FlacReader()
		{
			Dispose(disposing: false);
		}

		public int Read(float[] buffer, int offset, int count)
		{
			return -1;
		}
	}
}
