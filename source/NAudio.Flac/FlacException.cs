using System;
using System.Runtime.Serialization;

namespace NAudio.Flac
{
	[Serializable]
	public class FlacException : Exception
	{
		public FlacLayer Layer
		{
			get;
			private set;
		}

		public FlacException(string message, FlacLayer layer)
			: base(message)
		{
			Layer = layer;
		}

		public FlacException(Exception innerexception, FlacLayer layer)
			: base("See innerexception", innerexception)
		{
			Layer = layer;
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Layer", Layer);
		}
	}
}
