using System;
using System.Collections.Generic;
using System.Globalization;

namespace NAudio.Flac
{
	public class TimestampTextFrame : MultiStringTextFrame
	{
		private List<DateTime> _dateTimes;

		public List<DateTime> DateTimes => _dateTimes ?? (_dateTimes = new List<DateTime>());

		public TimestampTextFrame(FrameHeader header)
			: base(header)
		{
		}

		protected override void Decode(byte[] content)
		{
			base.Decode(content);
			foreach (string @string in base.Strings)
			{
				if (@string == null)
				{
					throw new ID3Exception("Timestamp-String is null");
				}
				DateTime item;
				if (string.IsNullOrEmpty(@string))
				{
					item = DateTime.MinValue;
				}
				else
				{
					string formatString = GetFormatString(@string.Length);
					try
					{
						item = DateTime.ParseExact(@string, formatString, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
					}
					catch (FormatException innerException)
					{
						throw new ID3Exception($"Could not parse [{@string}] with format [{formatString}] to Datetime. For details see Innerexception", innerException);
					}
				}
				DateTimes.Add(item);
			}
		}

		public static string GetFormatString(int length)
		{
			switch (length)
			{
			case 4:
				return "yyyy";
			case 7:
				return "yyyy-MM";
			case 10:
				return "yyyy-MM-dd";
			case 13:
				return "yyyy-MM-ddTHH";
			case 16:
				return "yyyy-MM-ddTHH:mm";
			case 19:
				return "yyyy-MM-ddTHH:mm:ss";
			default:
				throw new ID3Exception("Invalid length of timestamp");
			}
		}
	}
}
