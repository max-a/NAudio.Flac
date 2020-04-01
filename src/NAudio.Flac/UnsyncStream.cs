using System;
using System.IO;

namespace NAudio.Flac
{
	public class UnsyncStream : Stream
	{
		private Stream _stream;

		private int _svalue;

		public override bool CanRead => true;

		public override bool CanSeek => false;

		public override bool CanWrite => false;

		public override long Length
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override long Position
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public UnsyncStream(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (!stream.CanRead)
			{
				throw new ArgumentException("stream not readable");
			}
			_stream = stream;
		}

		public override void Flush()
		{
			_stream.Flush();
		}

		public override int ReadByte()
		{
			int num = _stream.ReadByte();
			if (_svalue == 255 && num == 0)
			{
				num = _stream.ReadByte();
				if (num != 0 && num < 224 && num != -1)
				{
					throw new ID3Exception("Invalid Unsync-Byte found");
				}
			}
			_svalue = num;
			return num;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int num = 0;
			int num2 = offset;
			while (num2 < offset + count && num != -1)
			{
				num = _stream.ReadByte();
				if (_svalue == 255 && num == 0)
				{
					num = _stream.ReadByte();
					if (num != 0 && num < 224 && num != -1)
					{
						throw new ID3Exception("Invalid Unsync-Byte found");
					}
				}
				if (num != -1)
				{
					buffer[num2++] = (byte)(num & 0xFF);
				}
				_svalue = num;
			}
			return num2 - offset;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}
	}
}
