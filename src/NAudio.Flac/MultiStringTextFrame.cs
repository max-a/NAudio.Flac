using System.Collections.Generic;
using System.Text;

namespace NAudio.Flac
{
	public class MultiStringTextFrame : TextFrame
	{
		private List<string> _strings;

		public List<string> Strings => _strings ?? (_strings = new List<string>());

		public override string Text => Strings[0];

		public MultiStringTextFrame(FrameHeader header)
			: base(header)
		{
		}

		protected override void Decode(byte[] content)
		{
			int i = 1;
			Strings.Clear();
			Encoding encoding = ID3Utils.GetEncoding(content, 0, 1);
			int read;
			for (; i < content.Length; i += read)
			{
				read = 0;
				Strings.Add(ID3Utils.ReadString(content, i, -1, encoding, out read));
			}
			if (Strings.Count == 0)
			{
				Strings.Add(string.Empty);
			}
		}
	}
}
