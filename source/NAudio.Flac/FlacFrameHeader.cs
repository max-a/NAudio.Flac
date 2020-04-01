using System;
using System.IO;

namespace NAudio.Flac
{
	public sealed class FlacFrameHeader
	{
		private int _blocksizeHint;

		private int _sampleRateHint;

		internal bool PrintErrors = true;

		public int BlockSize
		{
			get;
			set;
		}

		public int SampleRate
		{
			get;
			set;
		}

		public int Channels
		{
			get;
			set;
		}

		public ChannelAssignment ChannelAssignment
		{
			get;
			set;
		}

		public int BitsPerSample
		{
			get;
			set;
		}

		public FlacNumberType NumberType
		{
			get;
			set;
		}

		public ulong SampleNumber
		{
			get;
			set;
		}

		public uint FrameNumber
		{
			get;
			set;
		}

		public byte CRC8
		{
			get;
			set;
		}

		public bool DoCRC
		{
			get;
			private set;
		}

		public bool HasError
		{
			get;
			private set;
		}

		public long StreamPosition
		{
			get;
			private set;
		}

		public FlacFrameHeader(Stream stream)
			: this(stream, null, doCrc: true)
		{
		}

		public FlacFrameHeader(Stream stream, FlacMetadataStreamInfo streamInfo)
			: this(stream, streamInfo, doCrc: true)
		{
		}

		public FlacFrameHeader(Stream stream, FlacMetadataStreamInfo streamInfo, bool doCrc)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (!stream.CanRead)
			{
				throw new ArgumentException("stream is not readable");
			}
			DoCRC = doCrc;
			StreamPosition = stream.Position;
			HasError = !ParseHeader(stream, streamInfo);
		}

		public unsafe FlacFrameHeader(ref byte* buffer, FlacMetadataStreamInfo streamInfo, bool doCrc)
			: this(ref buffer, streamInfo, doCrc, logError: true)
		{
		}

		internal unsafe FlacFrameHeader(ref byte* buffer, FlacMetadataStreamInfo streamInfo, bool doCrc, bool logError)
		{
			PrintErrors = logError;
			DoCRC = doCrc;
			StreamPosition = -1L;
			HasError = !ParseHeader(ref buffer, streamInfo);
		}

		private unsafe bool ParseHeader(Stream stream, FlacMetadataStreamInfo streamInfo)
		{
			byte[] array = new byte[16];
			if (stream.Read(array, 0, array.Length) == array.Length)
			{
				fixed (byte* ptr = array)
				{
					byte* ptr2 = ptr;
					byte* headerBuffer = ptr;
					bool result = ParseHeader(ref headerBuffer, streamInfo);
					stream.Position -= array.Length - (headerBuffer - ptr2);
					return result;
				}
			}
			Error("Not able to read Flac header - EOF?", "FlacFrameHeader.ParseHeader(Stream, FlacMetadataStreamInfo)");
			return false;
		}

		private unsafe bool ParseHeader(ref byte* headerBuffer, FlacMetadataStreamInfo streamInfo)
		{
			int num = -1;
			if (*headerBuffer == byte.MaxValue && headerBuffer[1] >> 1 == 124)
			{
				if ((headerBuffer[1] & 2) != 0)
				{
					Error("Invalid FlacFrame. Reservedbit_0 is 1", "FlacFrameHeader.ParseHeader(byte*, FlacMetadataStreamInfo)");
					return false;
				}
				FlacBitReader flacBitReader = new FlacBitReader(headerBuffer, 0);
				num = headerBuffer[2] >> 4;
				int blockSize = -1;
				switch (num)
				{
				case 0:
					Error("Invalid Blocksize value: 0", "FlacFrameHeader.ParseHeader(byte*, FlacMetadataStreamInfo)");
					return false;
				case 1:
					blockSize = 192;
					break;
				case 2:
				case 3:
				case 4:
				case 5:
					blockSize = 576 << num - 2;
					break;
				default:
					switch (num)
					{
					case 6:
					case 7:
						_blocksizeHint = num;
						break;
					case 8:
					case 9:
					case 10:
					case 11:
					case 12:
					case 13:
					case 14:
					case 15:
						blockSize = 256 << num - 8;
						break;
					default:
						Error("Invalid Blocksize value: " + num, "FlacFrameHeader.ParseHeader(byte*, FlacMetadataStreamInfo)");
						return false;
					}
					break;
				}
				BlockSize = blockSize;
				num = (headerBuffer[2] & 0xF);
				int sampleRate = -1;
				switch (num)
				{
				case 0:
					if (streamInfo != null)
					{
						sampleRate = streamInfo.SampleRate;
						break;
					}
					Error("Missing Samplerate. Samplerate Index = 0 && streamInfoMetaData == null.", "FlacFrameHeader.ParseHeader(byte*, FlacMetadataStreamInfo)");
					return false;
				case 1:
				case 2:
				case 3:
				case 4:
				case 5:
				case 6:
				case 7:
				case 8:
				case 9:
				case 10:
				case 11:
					sampleRate = FlacConstant.SampleRateTable[num];
					break;
				default:
					if (num >= 12 && num <= 14)
					{
						_sampleRateHint = num;
						break;
					}
					Error("Invalid SampleRate value: " + num, "FlacFrameHeader.ParseHeader(byte*, FlacMetadataStreamInfo)");
					return false;
				}
				SampleRate = sampleRate;
				num = headerBuffer[3] >> 4;
				int num2 = -1;
				if ((num & 8) != 0)
				{
					num2 = 2;
					if ((num & 7) > 2 || (num & 7) < 0)
					{
						Error("Invalid ChannelAssignment", "FlacFrameHeader.ParseHeader(byte*, FlacMetadataStreamInfo)");
						return false;
					}
					ChannelAssignment = (ChannelAssignment)((num & 7) + 1);
				}
				else
				{
					num2 = num + 1;
					ChannelAssignment = ChannelAssignment.Independent;
				}
				Channels = num2;
				num = (headerBuffer[3] & 0xE) >> 1;
				int num3 = -1;
				switch (num)
				{
				case 0:
					if (streamInfo != null)
					{
						num3 = streamInfo.BitsPerSample;
						break;
					}
					Error("Missing BitsPerSample. Index = 0 && streamInfoMetaData == null.", "FlacFrameHeader.ParseHeader(byte*, FlacMetadataStreamInfo)");
					return false;
				default:
					Error("Invalid BitsPerSampleIndex", "FlacFrameHeader.ParseHeader(byte*, FlacMetadataStreamInfo)");
					return false;
				case 1:
				case 2:
				case 4:
				case 5:
				case 6:
					num3 = FlacConstant.BitPerSampleTable[num];
					break;
				}
				BitsPerSample = num3;
				if ((headerBuffer[3] & 1) != 0)
				{
					Error("Invalid FlacFrame. Reservedbit_1 is 1", "FlacFrameHeader.ParseHeader(byte*, FlacMetadataStreamInfo)");
					return false;
				}
				flacBitReader.ReadBits(32);
				if ((headerBuffer[1] & 1) != 0 || (streamInfo != null && streamInfo.MinBlockSize != streamInfo.MaxBlockSize))
				{
					if (!flacBitReader.ReadUTF8_64(out ulong result) || result == ulong.MaxValue)
					{
						Error("Invalid UTF8 Samplenumber coding.", "FlacFrameHeader.ParseHeader(byte*, FlacMetadataStreamInfo)");
						return false;
					}
					NumberType = FlacNumberType.SampleNumber;
					SampleNumber = result;
				}
				else
				{
					if (!flacBitReader.ReadUTF8_32(out uint result2) || result2 == uint.MaxValue)
					{
						Error("Invalid UTF8 Framenumber coding.", "FlacFrameHeader.ParseHeader(byte*, FlacMetadataStreamInfo)");
						return false;
					}
					NumberType = FlacNumberType.FrameNumber;
					FrameNumber = result2;
				}
				if (_blocksizeHint != 0)
				{
					num = (int)flacBitReader.ReadBits(8);
					if (_blocksizeHint == 7)
					{
						num = ((num << 8) | (int)flacBitReader.ReadBits(8));
					}
					BlockSize = num + 1;
				}
				if (_sampleRateHint != 0)
				{
					num = (int)flacBitReader.ReadBits(8);
					if (_sampleRateHint != 12)
					{
						num = ((num << 8) | (int)flacBitReader.ReadBits(8));
					}
					if (_sampleRateHint == 12)
					{
						SampleRate = num * 1000;
					}
					else if (_sampleRateHint == 13)
					{
						SampleRate = num;
					}
					else
					{
						SampleRate = num * 10;
					}
				}
				if (DoCRC)
				{
					byte b = NAudio.Flac.CRC8.Instance.CalcCheckSum(flacBitReader.Buffer, 0, flacBitReader.Position);
					CRC8 = (byte)flacBitReader.ReadBits(8);
					if (CRC8 != b)
					{
						Error("CRC8 missmatch", "FlacFrameHeader.ParseHeader(byte*, FlacMetadataStreamInfo)");
						return false;
					}
				}
				headerBuffer += flacBitReader.Position;
				return true;
			}
			Error("Invalid Syncbits", "FlacFrameHeader.ParseHeader(byte*, FlacMetadataStreamInfo)");
			return false;
		}

		internal void Error(string msg, string location)
		{
			_ = PrintErrors;
		}

		public bool CompareTo(FlacFrameHeader header)
		{
			if (BitsPerSample == header.BitsPerSample && Channels == header.Channels)
			{
				return SampleRate == header.SampleRate;
			}
			return false;
		}
	}
}
