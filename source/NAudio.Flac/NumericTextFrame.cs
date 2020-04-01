namespace NAudio.Flac
{
	public class NumericTextFrame : MultiStringTextFrame
	{
		public NumericTextFrame(FrameHeader header)
			: base(header)
		{
		}

		protected override void Decode(byte[] content)
		{
			base.Decode(content);
			foreach (string @string in base.Strings)
			{
				if (!char.IsNumber(@string, 0))
				{
					throw new ID3Exception("Invalid value: \"{0}\". Only numbers are allowed", @string);
				}
			}
		}
	}
}
