using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UCTask.Worker
{
	/// <summary>
	/// An abstract base class that provides basic thread, start and stop operations.
	/// </summary>
	public abstract partial class WorkerBase : IDisposable
	{
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
		public void Start()
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
		public void Stop()
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
				OnRunStarting(cancellationToken);
				DependentWorkers.StartAll();

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
				DependentWorkers.StopAll();
				OnRunEnding();
				WorkerState = WorkerState.Stopped;
			}
		}

		/// <summary>
		/// Implementing classes can override <see cref="OnRunStarting"/> which is called at the beginning of a start operation. <see cref="Start"/>
		/// </summary>
		protected virtual void OnRunStarting(CancellationToken cancellationToken) { }

		/// <summary>
		/// When overrided in a derived class, this method is executed with each cycle of the worker.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token that is used to cancel the task operation.</param>
		protected virtual void Cycle(CancellationToken cancellationToken) { }

		/// <summary>
		/// Implementing classes can override <see cref="OnRunEnding"/> which is called at the beginning of a stop operation. <see cref="Stop"/>
		/// </summary>
		protected virtual void OnRunEnding() { }

		/// <summary>
		/// Gets the dependent worker dictionary.
		/// </summary>
		protected DependentWorkerDictionary DependentWorkers { get; } = new DependentWorkerDictionary();
	}
}
