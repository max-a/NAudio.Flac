using System;
using System.Runtime.InteropServices;

namespace NAudio.Flac
{
	public class BitReader : IDisposable
	{
		private unsafe readonly byte* _storedBuffer;

		private int _bitoffset;

		private unsafe byte* _buffer;

		private uint _cache;

		private GCHandle _hBuffer;

		private int _position;

		protected internal uint Cache => _cache;

		public unsafe byte* Buffer => _storedBuffer;

		public int Position => _position;

		public unsafe BitReader(byte[] buffer, int offset)
		{
			if (buffer == null || buffer.Length == 0)
			{
				throw new ArgumentException("buffer is null or has no elements", "buffer");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			_hBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			_buffer = (_storedBuffer = (byte*)_hBuffer.AddrOfPinnedObject().ToPointer() + offset);
			_cache = PeekCache();
		}

		public unsafe BitReader(byte* buffer, int offset)
		{
			if (new IntPtr(buffer) == IntPtr.Zero)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			int num = offset / 8;
			_buffer = (_storedBuffer = buffer + num);
			_bitoffset = offset % 8;
			_cache = PeekCache();
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		private unsafe uint PeekCache()
		{
			byte* buffer = _buffer;
			return (uint)((((*(buffer++) << 8) + *(buffer++) << 8) + *(buffer++) << 8) + *(buffer++) << _bitoffset);
		}

		public void SeekBytes(int bytes)
		{
			if (bytes <= 0)
			{
				throw new ArgumentOutOfRangeException("bytes");
			}
			SeekBits(bytes * 8);
		}

		public unsafe void SeekBits(int bits)
		{
			if (bits <= 0)
			{
				throw new ArgumentOutOfRangeException("bits");
			}
			int num = _bitoffset + bits;
			_buffer += num >> 3;
			_bitoffset = (num & 7);
			_cache = PeekCache();
			_position += num >> 3;
		}

		public uint ReadBits(int bits)
		{
			if (bits <= 0 || bits > 32)
			{
				throw new ArgumentOutOfRangeException("bits", "bits has to be a value between 1 and 32");
			}
			uint num = _cache >> 32 - bits;
			if (bits <= 24)
			{
				SeekBits(bits);
				return num;
			}
			SeekBits(24);
			num |= _cache >> 56 - bits;
			SeekBits(bits - 24);
			return num;
		}

		public int ReadBitsSigned(int bits)
		{
			if (bits <= 0 || bits > 32)
			{
				throw new ArgumentOutOfRangeException("bits", "bits has to be a value between 1 and 32");
			}
			return (int)(ReadBits(bits) << 32 - bits) >> 32 - bits;
		}

		public ulong ReadBits64(int bits)
		{
			if (bits <= 0 || bits > 64)
			{
				throw new ArgumentOutOfRangeException("bits", "bits has to be a value between 1 and 32");
			}
			ulong num = ReadBits(Math.Min(24, bits));
			if (bits <= 24)
			{
				return num;
			}
			bits -= 24;
			num = ((num << bits) | ReadBits(Math.Min(24, bits)));
			if (bits <= 24)
			{
				return num;
			}
			bits -= 24;
			return (num << bits) | ReadBits(bits);
		}

		public long ReadBits64Signed(int bits)
		{
			if (bits <= 0 || bits > 64)
			{
				throw new ArgumentOutOfRangeException("bits", "bits has to be a value between 1 and 32");
			}
			return (long)(ReadBits64(bits) << 64 - bits) >> 64 - bits;
		}

		public short ReadInt16()
		{
			return (short)ReadBitsSigned(16);
		}

		public ushort ReadUInt16()
		{
			return (ushort)ReadBits(16);
		}

		public int ReadInt32()
		{
			return ReadBitsSigned(32);
		}

		public uint ReadUInt32()
		{
			return ReadBits(32);
		}

		public ulong ReadUInt64()
		{
			return ReadBits64(64);
		}

		public long ReadInt64()
		{
			return ReadBits64Signed(64);
		}

		public bool ReadBit()
		{
			return ReadBitI() == 1;
		}

		public int ReadBitI()
		{
			uint result = _cache >> 31;
			SeekBits(1);
			return (int)result;
		}

		public void Flush()
		{
			if (_bitoffset > 0 && _bitoffset <= 8)
			{
				SeekBits(8 - _bitoffset);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_hBuffer.IsAllocated)
			{
				_hBuffer.Free();
			}
		}

		~BitReader()
		{
			Dispose(disposing: false);
		}
	}
}
