using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace JPB.Tasking.TaskManagement.Threading
{
	/// <summary>
	///		Creates a Queue of Actions that will be called async as they are added. By defining the <para>MaxRunPerKey</para> you define how many actions
	///		should be run with the same key
	/// </summary>
	public class MultiTaskDispatcher : SerialTaskDispatcherBase
	{
		private readonly int _maxRunPerKey;
		
		///  <summary>
		/// 		Creates a new Instance of the SingelSerielTaskFactory and defines how many actions with the same key should be queued
		///  </summary>
		/// <param name="keepRunning"></param>
		/// <param name="maxRunPerKey"></param>
		public MultiTaskDispatcher(bool keepRunning, int maxRunPerKey = 1, [CallerMemberName] string namedConsumer = null) : base(keepRunning)
		{
			_namedConsumer = namedConsumer;
			_maxRunPerKey = maxRunPerKey;
			ConcurrentQueue = new ConcurrentQueue<Tuple<Action, object>>();
		}

		/// <summary>
		///		The Queue of Actions
		/// </summary>
		public ConcurrentQueue<Tuple<Action, object>> ConcurrentQueue { get; private set; }

		/// <summary>
		///		Enqueues a new Action and starts the Worker
		/// </summary>
		/// <param name="action"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool TryAdd(Action action, object key)
		{
			return TryAdd(action, key, _maxRunPerKey);
		}

		/// <summary>
		///		Enqueues the Action if its not the <para>max</para> action in the Queue
		/// </summary>
		/// <param name="action"></param>
		/// <param name="key"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public bool TryAdd(Action action, object key, int max)
		{
			//nested usage is not allowed
			if (_thread == Thread.CurrentThread)
			{
				action();
				return true;
			}

			if (key != null && ConcurrentQueue.Count(s => s.Item2.Equals(key)) > max)
			{
				return false;
			}

			ConcurrentQueue.Enqueue(new Tuple<Action, object>(action, key));
			StartScheduler();
			return true;
		}

		/// <inheritdoc />
		protected override Action GetNext()
		{
			if (!HasNext())
			{
				return null;
			}
			Tuple<Action, object> next;
			ConcurrentQueue.TryDequeue(out next);
			return next?.Item1;
		}

		/// <inheritdoc />
		protected override bool HasNext()
		{
			return !ConcurrentQueue.IsEmpty;
		}
	}
}