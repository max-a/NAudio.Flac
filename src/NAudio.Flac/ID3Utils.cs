using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;

namespace NAudio.Flac
{
	internal static class ID3Utils
	{
		public static readonly Encoding Iso88591 = Encoding.GetEncoding("ISO-8859-1");

		public static readonly Encoding Utf16 = new UnicodeEncoding(bigEndian: false, byteOrderMark: true);

		public static readonly Encoding Utf16Big = new UnicodeEncoding(bigEndian: true, byteOrderMark: true);

		public static readonly Encoding Utf8 = new UTF8Encoding();

		public const string MimeURL = "-->";

		public static int ReadInt32(byte[] array, int offset, bool sync, int length)
		{
			int num = 0;
			for (int i = offset; i < offset + length; i++)
			{
				if (sync)
				{
					if ((array[i] & 0x80) == 128)
					{
						throw new ID3Exception("Unknown error");
					}
					num = ((num << 7) | array[i]);
				}
				else
				{
					num = ((num << 8) | array[i]);
				}
			}
			return Math.Max(num, 0);
		}

		public static int ReadInt32(byte[] array, int offset, bool sync)
		{
			return ReadInt32(array, offset, sync, 4);
		}

		public static int ReadInt32(Stream stream, bool sync, int length)
		{
			byte[] array = new byte[4];
			if (stream.Read(array, 0, array.Length) < array.Length)
			{
				throw new EndOfStreamException();
			}
			return ReadInt32(array, 0, sync, length);
		}

		public static int ReadInt32(Stream stream, bool sync)
		{
			return ReadInt32(stream, sync, 4);
		}

		public static byte[] Read(Stream stream, int count)
		{
			byte[] array = new byte[count];
			if (stream.Read(array, 0, count) < count)
			{
				throw new EndOfStreamException();
			}
			return array;
		}

		public static string ReadString(byte[] buffer, int offset, int count, Encoding encoding)
		{
			int read;
			return ReadString(buffer, offset, count, encoding, out read);
		}

		public static string ReadString(byte[] buffer, int offset, int count, Encoding encoding, out int read)
		{
			int num = (encoding != Utf16 && encoding != Utf16Big) ? 1 : 2;
			if (count == -1)
			{
				count = buffer.Length;
			}
			int num2 = SeekPreamble(buffer, offset, count, encoding);
			int num3 = CalculateStringLength(buffer, offset, count, num);
			string @string = encoding.GetString(buffer, offset, num3);
			read = 0;
			read += num2 - offset;
			read += num3;
			read += num;
			return @string;
		}

		public static Encoding GetEncoding(byte[] buffer, int offset, int stringOffset)
		{
			switch (buffer[offset])
			{
			case 0:
				return Iso88591;
			case 1:
				if (buffer.Length < stringOffset + 2)
				{
					throw new ArgumentException("buffer to small");
				}
				if (buffer[stringOffset] == 254 && buffer[stringOffset + 1] == byte.MaxValue)
				{
					return Utf16Big;
				}
				if (buffer[stringOffset] == byte.MaxValue && buffer[stringOffset + 1] == 254)
				{
					return Utf16;
				}
				throw new ID3Exception("Can't detected UTF encoding");
			case 2:
				return Utf16Big;
			case 3:
				return Utf8;
			default:
				throw new ID3Exception("Invalid Encodingbyte");
			}
		}

		private static int CalculateStringLength(byte[] buffer, int offset, int count, int sizeofsymbol)
		{
			int i = offset;
			for (int num = sizeofsymbol - 1; i < Math.Min(buffer.Length, count + offset) && (buffer[i] != 0 || buffer[i + num] != 0); i += sizeofsymbol)
			{
			}
			return i - offset;
		}

		private static int SeekPreamble(byte[] buffer, int offset, int count, Encoding e)
		{
			byte[] preamble = e.GetPreamble();
			if (preamble.Length + offset > buffer.Length || preamble.Length > count)
			{
				return offset;
			}
			int i;
			for (i = 0; i < preamble.Length && preamble[i] == buffer[i + offset]; i++)
			{
			}
			if (i == preamble.Length)
			{
				return offset + i;
			}
			return offset;
		}

		public static Image DecodeImage(byte[] rawdata, string mimetype)
		{
			Stream stream = (!(mimetype.Trim() == "-->")) ? new MemoryStream(rawdata, writable: false) : new MemoryStream(new WebClient().DownloadData(GetURL(rawdata, mimetype)));
			return Image.FromStream(stream);
		}

		public static string GetURL(byte[] RawData, string MimeType)
		{
			if (RawData == null)
			{
				throw new InvalidOperationException("Decode the frame first");
			}
			if (MimeType != "-->")
			{
				throw new InvalidOperationException("MimeType != -->");
			}
			return ReadString(RawData, 0, -1, Iso88591);
		}
	}
}
