using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using JPB.Tasking.TaskManagement;

namespace JPB.WPFToolsAwesome.TaskManagement.Threading
{
	public class BlockingTaskDispatcher : SerialTaskDispatcherBase
	{
		public BlockingTaskDispatcher(bool keepRunning) : base(keepRunning)
		{
			_actions = new ConcurrentQueue<Func<object>>();
		}

		private readonly ConcurrentQueue<Func<object>> _actions;
		public static readonly TimeSpan InfiniteTimeSpan = new TimeSpan(0, 0, 0, 0, -1);

		public async Task EnqueueAsync(Action action)
		{
			var waiter = new ManualResetEventSlim();
			_actions.Enqueue(() =>
			{
				action();
				waiter.Set();
				return null;
			});
			await waiter.WaitHandle.WaitOneAsync(InfiniteTimeSpan);
		}

		public async Task EnqueueAsync(Func<Task> action)
		{
			var waiter = new ManualResetEventSlim();
			_actions.Enqueue(() =>
			{
				action().Wait();
				waiter.Set();
				return null;
			});
			await waiter.WaitHandle.WaitOneAsync(InfiniteTimeSpan);
		}

		public void Enqueue(Action action)
		{
			var waiter = new ManualResetEventSlim();
			_actions.Enqueue(() =>
			{
				action();
				waiter.Set();
				return null;
			});
			waiter.Wait();
		}

		public void Enqueue(Func<Task> action)
		{
			var waiter = new ManualResetEventSlim();
			_actions.Enqueue(() =>
			{
				action().Wait();
				waiter.Set();
				return null;
			});
			waiter.Wait();
		}

		public void EnqueueNonBlocking(Action action)
		{
			_actions.Enqueue(() =>
			{
				action();
				return null;
			});
		}

		public void EnqueueNonBlocking(Func<Task> action)
		{
			_actions.Enqueue(() =>
			{
				action().Wait();
				return null;
			});
		}

		protected override Func<object> GetNext()
		{
			Func<object> next;
			_actions.TryDequeue(out next);
			return next;
		}

		protected override bool HasNext()
		{
			return !_actions.IsEmpty;
		}
	}
}
