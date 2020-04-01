using System;

namespace NAudio.Flac
{
	public class ID3Exception : Exception
	{
		public ID3Exception(string message, params object[] args)
			: this(string.Format(message, args))
		{
		}

		public ID3Exception(string message)
			: base(message)
		{
		}

		public ID3Exception(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
