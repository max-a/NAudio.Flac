using System.Text;

namespace NAudio.Flac
{
	public class TextFrame : Frame
	{
		public virtual string Text
		{
			get;
			protected set;
		}

		public TextFrame(FrameHeader header)
			: base(header)
		{
		}

		protected override void Decode(byte[] content)
		{
			Decode(content, 0, -1, ID3Utils.Iso88591, out int _);
		}

		protected void Decode(byte[] content, int offset, int count, Encoding encoding, out int read)
		{
			if (content.Length == 0)
			{
				throw new ID3Exception("Contentlength is zero");
			}
			Text = ID3Utils.ReadString(content, 0, content.Length - 1, ID3Utils.Iso88591, out read);
		}
	}
}
