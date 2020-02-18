using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UCComs.Worker
{
	public abstract class WorkerBase
	{
		private readonly object _dependentLocker = new object();
		private readonly List<WorkerBase> _dependentWorkers = new List<WorkerBase>();

		protected readonly object StartStopLocker = new object();
		protected readonly CancellationTokenSource TokenSource = new CancellationTokenSource();
		protected Task WorkerTask;

		protected WorkerState WorkerState { get; private set; } = WorkerState.Stopped;

		protected void StartWorker()
		{
			if (WorkerState != WorkerState.Stopped)
				return;
			lock (StartStopLocker)
			{
				if (WorkerState != WorkerState.Stopped)
					return;

				WorkerTask = Task.Factory.StartNew(() => Run(TokenSource.Token), TokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
			}
		}

		protected void StopWorker()
		{
			if (WorkerState != WorkerState.Started)
				return;
			lock (StartStopLocker)
			{
				if (WorkerState != WorkerState.Started)
					return;

				WorkerState = WorkerState.Stopping;
				if (!WorkerTask.Wait(20, TokenSource.Token))
					TokenSource.Cancel(true);
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

		protected virtual void OnRunStarting() { }
		protected virtual void Cycle(CancellationToken cancellationToken) { }
		protected virtual void OnRunEnding() { }

		protected void AddDependent(WorkerBase dependent)
		{
			if (dependent == null)
				throw new ArgumentNullException(nameof(dependent));

			lock (_dependentLocker)
			{
				_dependentWorkers.Add(dependent);
			}
		}

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
