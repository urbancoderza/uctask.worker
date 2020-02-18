using System;
using System.Runtime.Serialization;

namespace UCTask.Worker
{
	/// <summary>
	/// Represents errors that occur during usage of the <see cref="WorkerBase"/>.
	/// </summary>
	[Serializable]
	public class WorkerException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WorkerException"/> class.
		/// </summary>
		public WorkerException()
		{ }
		/// <summary>
		/// Initializes a new instance of the <see cref="WorkerException"/> class with a specified error message.
		/// </summary>
		public WorkerException(string message) : base(message)
		{ }
		/// <summary>
		/// Initializes a new instance of the <see cref="WorkerException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		public WorkerException(string message, Exception innerException) : base(message, innerException)
		{ }
		/// <summary>
		/// Initializes a new instance of the <see cref="WorkerException"/> class with serialized data.
		/// </summary>
		protected WorkerException(SerializationInfo info, StreamingContext context) : base(info, context)
		{ }
	}
}
