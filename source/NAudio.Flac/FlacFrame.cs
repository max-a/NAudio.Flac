using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace NAudio.Flac
{
	public sealed class FlacFrame
	{
		private List<FlacSubFrameData> _data;

		private Stream _stream;

		private FlacMetadataStreamInfo _streamInfo;

		private GCHandle _handle1;

		private GCHandle _handle2;

		private int[] _destBuffer;

		private int[] _residualBuffer;

		public FlacFrameHeader Header
		{
			get;
			private set;
		}

		public ushort Crc16
		{
			get;
			private set;
		}

		public bool HasError
		{
			get;
			private set;
		}

		public static FlacFrame FromStream(Stream stream)
		{
			return new FlacFrame(stream);
		}

		public static FlacFrame FromStream(Stream stream, FlacMetadataStreamInfo streamInfo)
		{
			return new FlacFrame(stream, streamInfo);
		}

		private FlacFrame(Stream stream)
			: this(stream, null)
		{
		}

		private FlacFrame(Stream stream, FlacMetadataStreamInfo streamInfo)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (!stream.CanRead)
			{
				throw new ArgumentException("Stream is not readable");
			}
			_stream = stream;
			_streamInfo = streamInfo;
		}

		public bool NextFrame()
		{
			Decode(_stream, _streamInfo);
			return !HasError;
		}

		private void Decode(Stream stream, FlacMetadataStreamInfo streamInfo)
		{
			Header = new FlacFrameHeader(stream, streamInfo);
			_stream = stream;
			_streamInfo = streamInfo;
			HasError = Header.HasError;
			if (!HasError)
			{
				ReadSubFrames();
				FreeBuffers();
			}
		}

		private unsafe void ReadSubFrames()
		{
			List<FlacSubFrameBase> list = new List<FlacSubFrameBase>();
			List<FlacSubFrameData> list2 = _data = AllocOuputMemory();
			byte[] array = new byte[131072];
			if (_streamInfo.MaxFrameSize * Header.Channels * Header.BitsPerSample * 2 >> 3 > array.Length)
			{
				array = new byte[(_streamInfo.MaxFrameSize * Header.Channels * Header.BitsPerSample * 2 >> 3) - 16];
			}
			int num = _stream.Read(array, 0, (int)Math.Min(array.Length, _stream.Length - _stream.Position));
			fixed (byte* buffer = array)
			{
				FlacBitReader flacBitReader = new FlacBitReader(buffer, 0);
				for (int i = 0; i < Header.Channels; i++)
				{
					int num2 = Header.BitsPerSample;
					if (Header.ChannelAssignment == ChannelAssignment.MidSide || Header.ChannelAssignment == ChannelAssignment.LeftSide)
					{
						num2 += i;
					}
					else if (Header.ChannelAssignment == ChannelAssignment.RightSide)
					{
						num2 += 1 - i;
					}
					FlacSubFrameBase subFrame = FlacSubFrameBase.GetSubFrame(flacBitReader, list2[i], Header, num2);
					list.Add(subFrame);
				}
				flacBitReader.Flush();
				Crc16 = (ushort)flacBitReader.ReadBits(16);
				_stream.Position -= num - flacBitReader.Position;
				SamplesToBytes(_data);
			}
		}

		private unsafe void SamplesToBytes(List<FlacSubFrameData> data)
		{
			if (Header.ChannelAssignment == ChannelAssignment.LeftSide)
			{
				for (int i = 0; i < Header.BlockSize; i++)
				{
					data[1].DestBuffer[i] = data[0].DestBuffer[i] - data[1].DestBuffer[i];
				}
			}
			else if (Header.ChannelAssignment == ChannelAssignment.RightSide)
			{
				for (int j = 0; j < Header.BlockSize; j++)
				{
					data[0].DestBuffer[j] += data[1].DestBuffer[j];
				}
			}
			else if (Header.ChannelAssignment == ChannelAssignment.MidSide)
			{
				for (int k = 0; k < Header.BlockSize; k++)
				{
					int num = data[0].DestBuffer[k] << 1;
					int num2 = data[1].DestBuffer[k];
					num |= (num2 & 1);
					data[0].DestBuffer[k] = num + num2 >> 1;
					data[1].DestBuffer[k] = num - num2 >> 1;
				}
			}
		}

		public unsafe int GetBuffer(ref byte[] buffer, int offset)
		{
			int num = Header.BlockSize * Header.Channels * ((Header.BitsPerSample + 7) / 2);
			if (buffer == null || buffer.Length < num)
			{
				buffer = new byte[num];
			}
			fixed (byte* ptr = buffer)
			{
				byte* ptr2 = ptr;
				if (Header.BitsPerSample == 8)
				{
					for (int i = 0; i < Header.BlockSize; i++)
					{
						for (int j = 0; j < Header.Channels; j++)
						{
							*(ptr2++) = (byte)(_data[j].DestBuffer[i] + 128);
						}
					}
				}
				else if (Header.BitsPerSample == 16)
				{
					for (int k = 0; k < Header.BlockSize; k++)
					{
						for (int l = 0; l < Header.Channels; l++)
						{
							short num2 = (short)_data[l].DestBuffer[k];
							*(ptr2++) = (byte)(num2 & 0xFF);
							*(ptr2++) = (byte)((num2 >> 8) & 0xFF);
						}
					}
				}
				else
				{
					if (Header.BitsPerSample != 24)
					{
						throw new FlacException("FlacFrame::GetBuffer: Invalid Flac-BitsPerSample: " + Header.BitsPerSample + ".", FlacLayer.Frame);
					}
					for (int m = 0; m < Header.BlockSize; m++)
					{
						for (int n = 0; n < Header.Channels; n++)
						{
							int num3 = _data[n].DestBuffer[m];
							*(ptr2++) = (byte)(num3 & 0xFF);
							*(ptr2++) = (byte)((num3 >> 8) & 0xFF);
							*(ptr2++) = (byte)((num3 >> 16) & 0xFF);
						}
					}
				}
				return (int)(ptr2 - ptr);
			}
		}

		private unsafe List<FlacSubFrameData> AllocOuputMemory()
		{
			if (_destBuffer == null || _destBuffer.Length < Header.Channels * Header.BlockSize)
			{
				_destBuffer = new int[Header.Channels * Header.BlockSize];
			}
			if (_residualBuffer == null || _residualBuffer.Length < Header.Channels * Header.BlockSize)
			{
				_residualBuffer = new int[Header.Channels * Header.BlockSize];
			}
			List<FlacSubFrameData> list = new List<FlacSubFrameData>();
			for (int i = 0; i < Header.Channels; i++)
			{
				fixed (int* ptr = _destBuffer)
				{
					fixed (int* ptr2 = _residualBuffer)
					{
						_handle1 = GCHandle.Alloc(_destBuffer, GCHandleType.Pinned);
						_handle2 = GCHandle.Alloc(_residualBuffer, GCHandleType.Pinned);
						FlacSubFrameData item = new FlacSubFrameData
						{
							DestBuffer = ptr + i * Header.BlockSize,
							ResidualBuffer = ptr2 + i * Header.BlockSize
						};
						list.Add(item);
					}
				}
			}
			return list;
		}

		public void FreeBuffers()
		{
			if (_handle1.IsAllocated)
			{
				_handle1.Free();
			}
			if (_handle2.IsAllocated)
			{
				_handle2.Free();
			}
		}

		~FlacFrame()
		{
			FreeBuffers();
		}
	}
}
