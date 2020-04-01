using System;
using System.IO;

namespace NAudio.Flac
{
	public abstract class Frame
	{
		public FrameHeader Header
		{
			get;
			private set;
		}

		public string FrameId => Header.FrameID;

		public int FrameSize => Header.FrameSize;

		public static Frame FromStream(Stream stream, ID3v2 tag)
		{
			bool result = false;
			FrameHeader frameHeader = new FrameHeader(stream, tag.Header.Version);
			long position = stream.Position + frameHeader.FrameSize;
			Frame result2 = FrameFactory.Instance.TryGetFrame(frameHeader, tag.Header.Version, stream, out result);
			stream.Position = position;
			return result2;
		}

		protected Frame(FrameHeader header)
		{
			Header = header;
		}

		public void DecodeContent(byte[] content)
		{
			if (content == null || content.Length < 1)
			{
				throw new ArgumentException("content is null or length < 1");
			}
			Decode(content);
		}

		protected abstract void Decode(byte[] content);
	}
}
