using System;
using System.Drawing;
using System.Text;

namespace NAudio.Flac
{
	public class CommercialFrame : Frame
	{
		public string Price
		{
			get;
			private set;
		}

		public string IsValidUntil
		{
			get;
			private set;
		}

		public string ContactURL
		{
			get;
			private set;
		}

		public ReceivedType ReceivedType
		{
			get;
			private set;
		}

		public string SellerName
		{
			get;
			private set;
		}

		public string Description
		{
			get;
			private set;
		}

		public string LogoMimeType
		{
			get;
			private set;
		}

		public Image Image
		{
			get;
			private set;
		}

		public CommercialFrame(FrameHeader header)
			: base(header)
		{
		}

		protected override void Decode(byte[] content)
		{
			int num = 1;
			Price = ID3Utils.ReadString(content, num, -1, ID3Utils.Iso88591, out int read);
			num += read;
			IsValidUntil = ID3Utils.ReadString(content, num, -1, ID3Utils.Iso88591, out read);
			num += read;
			ContactURL = ID3Utils.ReadString(content, num, -1, ID3Utils.Iso88591, out read);
			num += read;
			ReceivedType = (ReceivedType)content[num];
			num++;
			Encoding encoding = ID3Utils.GetEncoding(content, 0, num);
			num++;
			SellerName = ID3Utils.ReadString(content, num, -1, encoding, out read);
			num += read;
			Description = ID3Utils.ReadString(content, num, -1, encoding, out read);
			num += read;
			if (num < content.Length)
			{
				LogoMimeType = ID3Utils.ReadString(content, num, -1, ID3Utils.Iso88591, out read);
				num += read;
				byte[] array = new byte[content.Length - num];
				Array.Copy(content, num, array, 0, array.Length);
				Image = ID3Utils.DecodeImage(array, LogoMimeType);
			}
		}
	}
}
