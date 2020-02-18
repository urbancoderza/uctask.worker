using System;
using System.Runtime.Serialization;

namespace UCComs.Worker
{
	[Serializable]
	public class WorkerException : Exception
	{
		public WorkerException()
		{ }
		public WorkerException(string message) : base(message)
		{ }
		public WorkerException(string message, Exception innerException) : base(message, innerException)
		{ }
		protected WorkerException(SerializationInfo info, StreamingContext context) : base(info, context)
		{ }
	}
}
