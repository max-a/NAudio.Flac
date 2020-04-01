using System.Text;

namespace NAudio.Flac
{
	public class OwnershipFrame : TextFrame
	{
		public string Price
		{
			get;
			private set;
		}

		public string PurchaseDate
		{
			get;
			private set;
		}

		public OwnershipFrame(FrameHeader header)
			: base(header)
		{
		}

		protected override void Decode(byte[] content)
		{
			int num = 1;
			if (content.Length < 10)
			{
				throw new ID3Exception("Invalid Contentlength");
			}
			Price = ID3Utils.ReadString(content, num, -1, ID3Utils.Iso88591, out int read);
			num += read;
			PurchaseDate = ID3Utils.ReadString(content, num, 8, ID3Utils.Iso88591);
			num += 8;
			Encoding encoding = ID3Utils.GetEncoding(content, 0, num);
			Text = ID3Utils.ReadString(content, num, -1, encoding);
		}
	}
}
