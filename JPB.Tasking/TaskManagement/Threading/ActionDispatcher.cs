#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 11:06

#endregion

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace JPB.Tasking.TaskManagement.Threading
{
	/// <summary>
	///		Creates a Queue of Actions that will be called asyncrolly as they are added
	/// </summary>
	public class ActionDispatcher : SerialTaskDispatcherBase, IDisposable
	{
		/// <inheritdoc />
		public ActionDispatcher(bool keepRunning = false, [CallerMemberName] string namedConsumer = null) : base(keepRunning)
		{
			_namedConsumer = namedConsumer;
			ConcurrentQueue = new ConcurrentQueue<Action>();
			Timeout = DefaultTimeout;
		}
		/// <summary>
		/// Current queued Actions
		/// </summary>
		public ConcurrentQueue<Action> ConcurrentQueue { get; private set; }

		/// <summary>
		/// Adds an Action to the Queue and starts the scheduler
		/// </summary>
		/// <param name="action"></param>
		public void Add(Action action)
		{
			if (_isDisposed)
			{
				throw new ObjectDisposedException("The Instance of the " + nameof(ActionDispatcher) + " was disposed and cannot accept new Actions");
			}

			TryAdd(action);
		}

		/// <summary>
		/// Adds an Action to the Queue and starts the scheduler
		/// </summary>
		/// <param name="action"></param>
		public bool TryAdd(Action action)
		{
			if (_isDisposed)
			{
				return false;
			}

			if (Thread.CurrentThread == _thread)
			{
				action();
				return true;
			}

			ConcurrentQueue.Enqueue(action);
			StartScheduler();
			return true;
		}

		protected override Func<object> GetNext()
		{
			Action next;
			ConcurrentQueue.TryDequeue(out next);
			return next.WrapAsFunc();
		}

		protected override bool HasNext()
		{
			return !ConcurrentQueue.IsEmpty;
		}
	}
}
