using System;

namespace UCTask.Worker
{
	public abstract partial class WorkerBase
	{
		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Destroy(bool nativeOnly)
		{
			if (!nativeOnly)
			{
				if (_tokenSource != null)
					_tokenSource.Dispose();
				if (_workerTask != null)
					_workerTask.Dispose();
			}
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
			Destroy(!disposing);
		}
	}
}
