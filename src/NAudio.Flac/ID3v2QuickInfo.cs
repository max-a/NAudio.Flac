using System;
using System.Drawing;

namespace NAudio.Flac
{
	public class ID3v2QuickInfo
	{
		private ID3v2 _id3;

		public string Title
		{
			get
			{
				Frame frame;
				if ((frame = _id3[FrameID.Title]) != null)
				{
					return (frame as TextFrame).Text;
				}
				return string.Empty;
			}
		}

		public string Album
		{
			get
			{
				Frame frame;
				if ((frame = _id3[FrameID.Album]) != null)
				{
					return (frame as TextFrame).Text;
				}
				return string.Empty;
			}
		}

		public string Artist
		{
			get
			{
				Frame frame;
				if ((frame = _id3[FrameID.OriginalArtist]) != null)
				{
					return (frame as TextFrame).Text;
				}
				return string.Empty;
			}
		}

		public string LeadPerformers
		{
			get
			{
				Frame frame;
				if ((frame = _id3[FrameID.LeadPerformers]) != null)
				{
					return (frame as TextFrame).Text;
				}
				return string.Empty;
			}
		}

		public string Comments
		{
			get
			{
				Frame frame;
				if ((frame = _id3[FrameID.Comments]) != null)
				{
					return (frame as CommentAndLyricsFrame).Text;
				}
				return string.Empty;
			}
		}

		public Image Image
		{
			get
			{
				Frame frame;
				if ((frame = _id3[FrameID.AttachedPicutre]) != null)
				{
					return (frame as PictureFrame).Image;
				}
				return null;
			}
		}

		public int? Year
		{
			get
			{
				Frame frame;
				if ((frame = _id3[FrameID.Year]) != null && int.TryParse((frame as NumericTextFrame).Text, out int result))
				{
					return result;
				}
				return null;
			}
		}

		public int? TrackNumber
		{
			get
			{
				Frame frame;
				if ((frame = _id3[FrameID.Year]) != null && int.TryParse((frame as MultiStringTextFrame).Text, out int result))
				{
					return result;
				}
				return null;
			}
		}

		public int? OriginalReleaseYear
		{
			get
			{
				Frame frame;
				if ((frame = _id3[FrameID.OriginalReleaseYear]) != null)
				{
					return int.Parse((frame as NumericTextFrame).Text);
				}
				return null;
			}
		}

		public ID3Genre? Genre
		{
			get
			{
				MultiStringTextFrame multiStringTextFrame = _id3[FrameID.ContentType] as MultiStringTextFrame;
				if (multiStringTextFrame == null)
				{
					return null;
				}
				string text = multiStringTextFrame.Text;
				if (string.IsNullOrEmpty(text) || !text.StartsWith("(") || text.Length < 3)
				{
					try
					{
						return (ID3Genre)Enum.Parse(typeof(ID3Genre), text);
					}
					catch (Exception)
					{
						return null;
					}
				}
				int num = 1;
				string text2 = string.Empty;
				char c;
				do
				{
					c = text[num++];
					if (char.IsNumber(c))
					{
						text2 += c.ToString();
					}
				}
				while (num < text.Length && char.IsNumber(c));
				int result = 0;
				if (int.TryParse(text2, out result))
				{
					return (ID3Genre)result;
				}
				return null;
			}
		}

		public ID3v2QuickInfo(ID3v2 id3)
		{
			if (id3 == null)
			{
				throw new ArgumentNullException("id3");
			}
			_id3 = id3;
		}
	}
}
