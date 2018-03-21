#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 11:06

#endregion

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace JPB.Tasking.TaskManagement.Threading
{
    public class SerielTaskFactory : IDisposable
	{
		private readonly string _namedConsumer;
		private Thread _thread;
		private readonly object _lockRoot = new object();
		private bool _isDisposed;

		/// <summary>
		/// Default Timeout for disposing
		/// </summary>
		public static readonly TimeSpan DefaultTimeout = new TimeSpan(0, 0, 0, 5);

		/// <summary>
		/// Timeout for disposing
		/// </summary>
		public TimeSpan Timeout { get; set; }

		private volatile bool _isWorking;

		/// <summary>
		/// Will be invoked when the Status of the Current Thread has Changed
		/// </summary>
		public event EventHandler StateChanged;

		/// <summary>
		/// Will be invoked when any task has Failed
		/// </summary>
		public event EventHandler<Exception> TaskFailedAsync;

		/// <summary>
		/// Returns true if the Current Thread is working on any item inside the ConcurrentQueue
		/// </summary>
		// ReSharper disable once InconsistentlySynchronizedField
		public bool IsWorking => _isWorking;

		/// <summary>
		/// Ctor
		/// </summary>
		public SerielTaskFactory([CallerMemberName]string namedConsumer = null)
		{
			_namedConsumer = namedConsumer;
			ConcurrentQueue = new ConcurrentQueue<Action>();
			Timeout = DefaultTimeout;
		}
		/// <summary>
		/// Current enqued Actions
		/// </summary>
		public ConcurrentQueue<Action> ConcurrentQueue { get; set; }

		/// <summary>
		/// Adds an Action to the Queue and starts the scheduler
		/// </summary>
		/// <param name="action"></param>
		public void Add(Action action)
		{
			if (_isDisposed)
			{
				throw new ObjectDisposedException("The Instance of the " + nameof(SerielTaskFactory) + " was disposed and cannot accept new Actions");
			}

			ConcurrentQueue.Enqueue(action);
			StartScheduler();
		}

		private void StartScheduler()
		{
			lock (_lockRoot)
			{
				if (_isWorking)
					return;

				_isWorking = true;
				_thread = new Thread(Worker);
				_thread.Name = _namedConsumer;
				_thread.SetApartmentState(ApartmentState.MTA);
				_thread.Start();
			}
			OnStateChanged();
		}

		internal void Worker()
		{
			try
			{
				while (ConcurrentQueue.Any() && !_isDisposed)
				{
					Action action;
					if (ConcurrentQueue.TryDequeue(out action))
					{
						try
						{
							action.Invoke();
						}
						catch (Exception e)
						{
							OnTaskFailed(e);
						}
					}
				}
			}
			finally
			{
				lock (_lockRoot)
				{
					_isWorking = false;
				}
				OnStateChanged();
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_isDisposed = true;
			if (_thread != null)
			{
				//give the thread time to complete
				_thread.Join(Timeout);
				if (_thread.IsAlive)
				{
					//Thread seems to be waiting for a resource so interrupt all sleeps or wait handels
					_thread.Interrupt();
					//the Thread should have been listening!!
					_thread.Abort();
				}
				_thread = null;
			}
			
			StateChanged = null;
			TaskFailedAsync = null;
		}

		/// <summary>
		/// Raises the StateChanged Event
		/// </summary>
		protected virtual void OnStateChanged()
		{
			StateChanged?.Invoke(this, EventArgs.Empty);
		}
		/// <summary>
		/// Raises the TaskFailedAsync Event
		/// </summary>
		protected virtual void OnTaskFailed(Exception e)
		{
			var taskFailedEvent = TaskFailedAsync;
			if (taskFailedEvent != null)
			{
				Task.Run(() => taskFailedEvent.Invoke(this, e));
			}
		}
	}
}
