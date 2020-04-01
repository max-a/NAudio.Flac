using System.Text;

namespace NAudio.Flac
{
	public class UserDefiniedTextFrame : TextFrame
	{
		public string Description
		{
			get;
			private set;
		}

		public UserDefiniedTextFrame(FrameHeader header)
			: base(header)
		{
		}

		protected override void Decode(byte[] content)
		{
			if (content.Length == 0)
			{
				throw new ID3Exception("Contentlength is zero");
			}
			FrameIDFactory2.ID3v2FrameEntry frameInformation = base.Header.GetFrameInformation();
			bool num = frameInformation != null && frameInformation.ID == FrameID.UserURLLinkFrame;
			Encoding encoding = ID3Utils.GetEncoding(content, 0, 1);
			Encoding encoding2 = encoding;
			if (num)
			{
				encoding2 = ID3Utils.Iso88591;
			}
			Description = ID3Utils.ReadString(content, 1, -1, encoding, out int read);
			if (content.Length < read + 1)
			{
				throw new ID3Exception("Frame does not contain any text");
			}
			Decode(content, read + 1, -1, encoding2, out read);
		}
	}
}
