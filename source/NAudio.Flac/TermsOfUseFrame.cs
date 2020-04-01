using System.Text;

namespace NAudio.Flac
{
	public class TermsOfUseFrame : TextFrame
	{
		public string Language
		{
			get;
			private set;
		}

		public TermsOfUseFrame(FrameHeader header)
			: base(header)
		{
		}

		protected override void Decode(byte[] content)
		{
			int num = 0;
			Encoding encoding = ID3Utils.GetEncoding(content, 0, 4);
			num++;
			ID3Utils.ReadString(content, num, 3, ID3Utils.Iso88591);
			num += 3;
			Text = ID3Utils.ReadString(content, num, -1, encoding);
		}
	}
}
