using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAudio.Flac
{
	public class ID3v2 : IEnumerable<Frame>, IEnumerable
	{
		private Stream _stream;

		private ID3v2Header _header;

		private ID3v2ExtendedHeader _extendedHeader;

		private ID3v2Footer _footer;

		private List<Frame> _frames = new List<Frame>();

		private ID3v2QuickInfo _quickInfo;

		private byte[] _content;

		public ID3v2Header Header => _header;

		public ID3v2ExtendedHeader ExtendedHeader => _extendedHeader;

		public ID3v2Footer Footer => _footer;

		public ID3v2QuickInfo QuickInfo => _quickInfo ?? (_quickInfo = new ID3v2QuickInfo(this));

		public Frame this[FrameID id] => this[FrameIDFactory2.GetID(id, Header.Version)];

		public Frame this[string id] => _frames.Where((Frame o) => o.FrameId == id).FirstOrDefault();

		public static ID3v2 FromFile(string filename)
		{
			using (FileStream stream = File.OpenRead(filename))
			{
				return FromStream(stream);
			}
		}

		public static ID3v2 FromStream(Stream stream)
		{
			try
			{
				ID3v2 iD3v = new ID3v2(stream);
				if (iD3v.ReadData(stream, readData: true))
				{
					return iD3v;
				}
			}
			catch (Exception)
			{
				return null;
			}
			return null;
		}

		private static ID3v2 FromStream(Stream stream, bool readData)
		{
			ID3v2 iD3v = new ID3v2(stream);
			if (iD3v.ReadData(stream, readData))
			{
				return iD3v;
			}
			return null;
		}

		public static void SkipTag(Stream stream)
		{
			long position = stream.Position;
			if (FromStream(stream, readData: false) == null)
			{
				stream.Position = position;
			}
		}

		protected ID3v2(Stream stream)
		{
			_stream = stream;
			_frames = new List<Frame>();
		}

		private bool ReadData(Stream stream, bool readData)
		{
			if ((_header = ID3v2Header.FromStream(stream)) != null)
			{
				byte[] array = new byte[_header.DataLength];
				if (stream.Read(array, 0, array.Length) < array.Length)
				{
					return false;
				}
				if (_header.IsUnsync && _header.Version != ID3Version.ID3v2_4)
				{
					array = UnSyncBuffer(array);
				}
				_content = array;
				MemoryStream memoryStream = new MemoryStream(array);
				switch (_header.Version)
				{
				case ID3Version.ID3v2_2:
					Parse2(memoryStream);
					break;
				case ID3Version.ID3v2_3:
					Parse3(memoryStream);
					break;
				case ID3Version.ID3v2_4:
					Parse4(memoryStream);
					break;
				default:
					throw new ID3Exception("Invalid Version: [2.{0};{1}]", _header.RawVersion[0], _header.RawVersion[1]);
				}
				if (readData)
				{
					ReadFrames(memoryStream);
				}
				memoryStream.Dispose();
				return true;
			}
			return false;
		}

		private bool Parse2(Stream stream)
		{
			if ((_header.Flags & (ID3v2HeaderFlags)63) != 0)
			{
				throw new ID3Exception("Invalid headerflags: 0x{0}.", ((int)_header.Flags).ToString("x"));
			}
			return true;
		}

		private bool Parse3(Stream stream)
		{
			if ((_header.Flags & ID3v2HeaderFlags.ExtendedHeader) == ID3v2HeaderFlags.ExtendedHeader)
			{
				_extendedHeader = new ID3v2ExtendedHeader(stream, ID3Version.ID3v2_3);
			}
			if ((_header.Flags & (ID3v2HeaderFlags)31) != 0)
			{
				throw new ID3Exception("Invalid headerflags: 0x{0}.", ((int)_header.Flags).ToString("x"));
			}
			return true;
		}

		private bool Parse4(Stream stream)
		{
			if ((_header.Flags & ID3v2HeaderFlags.ExtendedHeader) == ID3v2HeaderFlags.ExtendedHeader)
			{
				_extendedHeader = new ID3v2ExtendedHeader(stream, ID3Version.ID3v2_4);
			}
			if ((_header.Flags & ID3v2HeaderFlags.FooterPresent) == ID3v2HeaderFlags.FooterPresent)
			{
				_footer = ID3v2Footer.FromStream(_stream);
				if (_footer == null)
				{
					throw new ID3Exception("Invalid Id3Footer.");
				}
			}
			if ((_header.Flags & (ID3v2HeaderFlags)15) != 0)
			{
				throw new ID3Exception("Invalid headerflags: 0x{0}.", ((int)_header.Flags).ToString("x"));
			}
			return true;
		}

		private void ReadFrames(Stream stream)
		{
			while (stream.Position < stream.Length && stream.ReadByte() != 0)
			{
				stream.Position--;
				Frame frame = Frame.FromStream(stream, this);
				if (frame != null)
				{
					_frames.Add(frame);
				}
			}
		}

		private byte[] UnSyncBuffer(byte[] buffer)
		{
			UnsyncStream unsyncStream = new UnsyncStream(new MemoryStream(buffer));
			byte[] array = new byte[buffer.Length];
			int num = unsyncStream.Read(array, 0, array.Length);
			if (num < array.Length)
			{
				byte[] array2 = new byte[num];
				Buffer.BlockCopy(array, 0, array2, 0, num);
				return array2;
			}
			return array;
		}

		public IEnumerator<Frame> GetEnumerator()
		{
			return _frames.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _frames.GetEnumerator();
		}
	}
}
