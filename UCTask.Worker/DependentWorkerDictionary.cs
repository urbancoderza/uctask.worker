using System;
using System.Collections.Generic;

namespace UCTask.Worker
{
	/// <summary>
	/// A class representing a dictionary for dependent workers. The keys of the disctionary is case-insensitive strings.
	/// </summary>
	public class DependentWorkerDictionary
	{
		private readonly Dictionary<string, WorkerBase> _workers = new Dictionary<string, WorkerBase>(StringComparer.OrdinalIgnoreCase);

		internal DependentWorkerDictionary()
		{ }

		/// <summary>
		/// Adds a dependent worker to the items collection.
		/// </summary>
		/// <param name="key">The unique case-insensitive key for the worker to be added.</param>
		/// <param name="dependentWorker">The worker to add.</param>
		/// <returns>A bool indicating whether the worker was added to the items collection or not.</returns>
		/// <remarks>If the key already exists in the dictionary, the worker will not be added and the method will return false.</remarks>
		public bool Add(string key, WorkerBase dependentWorker)
		{
			lock (_workers)
			{
				if (_workers.ContainsKey(key))
					return false;

				_workers.Add(key, dependentWorker);
				return true;
			}
		}

		/// <summary>
		/// Removes a worker from the items collection.
		/// </summary>
		/// <param name="key">The unique case-insensitive key for the worker to be removed.</param>
		/// <returns>If the key was found in the dictionary, returns the worker that was removed, else returns null.</returns>
		public WorkerBase Remove(string key)
		{
			lock (_workers)
			{
				if (!_workers.ContainsKey(key))
					return null;

				var toReturn = _workers[key];
				_workers.Remove(key);
				return toReturn; ;
			}
		}

		/// <summary>
		/// Determines if the <see cref="DependentWorkerDictionary"/> contains the specified key.
		/// </summary>
		/// <param name="key">The unique case-insensitive key for the worker to be searched.</param>
		/// <returns>A bool indicating whether the key exists in the dictionary or not.</returns>
		public bool ContainsKey(string key)
		{
			return _workers.ContainsKey(key);
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key of the value to get.</param>
		/// <returns>A <see cref="WorkerBase"/> associated with the specified key.</returns>
		public WorkerBase this[string key] => _workers[key];

		/// <summary>
		/// Starts each of the dependent workers if it is not already started.
		/// </summary>
		public void StartAll()
		{
			lock (_workers)
			{
				foreach (var worker in _workers.Values)
					worker.Start();
			}
		}

		/// <summary>
		/// Stops each of the dependent workers if it is not already started.
		/// </summary>
		public void StopAll()
		{
			lock (_workers)
			{
				foreach (var worker in _workers.Values)
					worker.Stop();
			}
		}
	}
}
