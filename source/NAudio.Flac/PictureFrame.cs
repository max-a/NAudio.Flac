using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;

namespace NAudio.Flac
{
	public class PictureFrame : Frame
	{
		private Image _image;

		private ID3Version _version;

		public string MimeType
		{
			get;
			private set;
		}

		public PictureFormat Format
		{
			get;
			set;
		}

		public string Description
		{
			get;
			private set;
		}

		internal byte[] RawData
		{
			get;
			private set;
		}

		public Image Image => _image ?? (_image = DecodeImage());

		private Image DecodeImage()
		{
			return ID3Utils.DecodeImage(RawData, MimeType);
		}

		public PictureFrame(FrameHeader header, ID3Version version)
			: base(header)
		{
			if (version == ID3Version.ID3v1)
			{
				throw new InvalidEnumArgumentException("version", (int)version, typeof(ID3Version));
			}
			_version = version;
		}

		protected override void Decode(byte[] content)
		{
			int num = 1;
			if (content.Length < 3)
			{
				throw new ID3Exception("Invalid contentlength id=0.");
			}
			int read;
			if (_version == ID3Version.ID3v2_2)
			{
				MimeType = ID3Utils.ReadString(content, num, 3, ID3Utils.Iso88591, out read);
				num += 3;
			}
			else
			{
				MimeType = ID3Utils.ReadString(content, 1, -1, ID3Utils.Iso88591, out read);
				num += read;
			}
			if (content.Length < num)
			{
				throw new ID3Exception("Invalid contentlength id=1.");
			}
			if (!Enum.IsDefined(typeof(PictureFormat), content[num]))
			{
				throw new ID3Exception("Invalid pictureformat: 0x{0}", content[num].ToString("x"));
			}
			Format = (PictureFormat)content[num++];
			if (content.Length < num)
			{
				throw new ID3Exception("Invalid contentlength id=2.");
			}
			Encoding encoding = ID3Utils.GetEncoding(content, 0, 2);
			Description = ID3Utils.ReadString(content, num, -1, encoding, out read);
			num += read;
			if (content.Length < num)
			{
				throw new ID3Exception("Invalid contentlength id=3.");
			}
			RawData = new byte[content.Length - num];
			Array.Copy(content, num, RawData, 0, RawData.Length);
		}

		public string GetURL()
		{
			return ID3Utils.GetURL(RawData, MimeType);
		}
	}
}
