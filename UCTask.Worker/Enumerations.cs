namespace UCTask.Worker
{
	/// <summary>
	/// An enumeration that represents the different states of a worker.
	/// </summary>
	public enum WorkerState
	{
		/// <summary>
		/// Indicates that the worker is in a 'Stopped' state.
		/// </summary>
		Stopped = 0,
		/// <summary>
		/// Indicates that the worker is in a 'Starting' state. That is, the worker is busy starting.
		/// </summary>
		Starting = 1,
		/// <summary>
		/// Indicates that the worker is in a 'Starting' state.
		/// </summary>
		Started = 2,
		/// <summary>
		/// Indicates that the worker is in a 'Stopping' state. That is, the worker is busy stopping.
		/// </summary>
		Stopping = 3
	}
}
