using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

[assembly: NeutralResourcesLanguage("en")]
namespace UCTask.Worker
{
	/// <summary>
	/// An abstract base class that provides basic thread, start and stop operations.
	/// </summary>
	public abstract partial class WorkerBase : IDisposable
	{
		private readonly object _dependentLocker = new object();
		private readonly List<WorkerBase> _dependentWorkers = new List<WorkerBase>();
		private Task _workerTask;
		private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
		private readonly object _startStopLocker = new object();

		/// <summary>
		/// Represents the state of the worker. <see cref="WorkerState"/>
		/// </summary>
		public WorkerState WorkerState { get; private set; } = WorkerState.Stopped;

		/// <summary>
		/// Starts the worker.
		/// </summary>
		protected void StartWorker()
		{
			if (WorkerState != WorkerState.Stopped)
				return;
			lock (_startStopLocker)
			{
				if (WorkerState != WorkerState.Stopped)
					return;

				_workerTask = Task.Factory.StartNew(() => Run(_tokenSource.Token), _tokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
			}
		}

		/// <summary>
		/// Stops the worker.
		/// </summary>
		protected void StopWorker()
		{
			if (WorkerState != WorkerState.Started)
				return;
			lock (_startStopLocker)
			{
				if (WorkerState != WorkerState.Started)
					return;

				WorkerState = WorkerState.Stopping;
				if (!_workerTask.Wait(20, _tokenSource.Token))
					_tokenSource.Cancel(true);
			}
		}

		private void Run(CancellationToken cancellationToken)
		{
			try
			{
				WorkerState = WorkerState.Starting;
				OnRunStarting();
				lock (_dependentLocker)
				{
					foreach (var dep in _dependentWorkers)
						dep.StartWorker();
				}
				WorkerState = WorkerState.Started;

				while (WorkerState == WorkerState.Started)
				{
					cancellationToken.ThrowIfCancellationRequested();
					Cycle(cancellationToken);
				}
			}
			finally
			{
				WorkerState = WorkerState.Stopping;
				lock (_dependentLocker)
				{
					foreach (var dep in _dependentWorkers)
						dep.StopWorker();
				}
				OnRunEnding();
				WorkerState = WorkerState.Stopped;
			}
		}

		/// <summary>
		/// Implementing classes can override <see cref="OnRunStarting"/> which is called at the beginning of a start operation. <see cref="StartWorker"/>
		/// </summary>
		protected virtual void OnRunStarting() { }

		/// <summary>
		/// When overrided in a derived class, this method is executed with each cycle of the worker.
		/// </summary>
		/// <param name="cancellationToken"></param>
		protected virtual void Cycle(CancellationToken cancellationToken) { }

		/// <summary>
		/// Implementing classes can override <see cref="OnRunEnding"/> which is called at the beginning of a stop operation. <see cref="StopWorker"/>
		/// </summary>
		protected virtual void OnRunEnding() { }

		/// <summary>
		/// Adds a dependent worker which is subject to the start and stop operations of this worker.
		/// </summary>
		/// <param name="dependent">The worker which is to be added.</param>
		protected void AddDependent(WorkerBase dependent)
		{
			if (dependent == null)
				throw new ArgumentNullException(nameof(dependent));

			lock (_dependentLocker)
			{
				_dependentWorkers.Add(dependent);
			}
		}

		/// <summary>
		/// Removes a dependent worker.
		/// </summary>
		/// <param name="dependent">The worker which is not to be removed.</param>
		protected void RemoveDependent(WorkerBase dependent)
		{
			if (dependent == null)
				throw new ArgumentNullException(nameof(dependent));

			lock (_dependentLocker)
			{
				_dependentWorkers.Remove(dependent);
			}
		}
	}
}
