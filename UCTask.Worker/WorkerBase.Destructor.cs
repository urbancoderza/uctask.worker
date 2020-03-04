using System;
using System.Threading;

namespace UCTask.Worker
{
	public abstract partial class WorkerBase
	{
		private int _disposed;

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Default destructor.
		/// </summary>
		~WorkerBase()
		{
			Dispose(false);
		}

		/// <summary>
		/// Releases the resources used by the component.
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
				{
					Stop();

					if (_workerTask != null)
					{
						if (_workerTask.IsCompleted)
							_workerTask.Dispose();
					}
					if (_tokenSource != null)
						_tokenSource.Dispose();
				}
			}
		}
	}
}
