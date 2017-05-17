using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JPB.WPFBase.MVVM.ViewModel
{
	public static class WaitHandleExtensions
	{
		public static Task AsTask<T>(this WaitHandle handle)
		{
			return AsTask<T>(handle, Timeout.InfiniteTimeSpan);
		}

		public static Task AsTask<T>(this WaitHandle handle, TimeSpan timeout)
		{
			var tcs = new TaskCompletionSource<T>();
			var registration = ThreadPool.RegisterWaitForSingleObject(handle, (state, timedOut) =>
			{
				var localTcs = (TaskCompletionSource<T>) state;
				if (timedOut)
					localTcs.TrySetCanceled();
				else
					localTcs.TrySetResult(default(T));
			}, tcs, timeout, true);
			tcs.Task.ContinueWith((_, state) => ((RegisteredWaitHandle) state).Unregister(null), registration,
				TaskScheduler.Default);
			return tcs.Task;
		}

		public static Task AsTask(this WaitHandle handle)
		{
			return AsTask(handle, Timeout.InfiniteTimeSpan);
		}

		public static Task AsTask(this WaitHandle handle, TimeSpan timeout)
		{
			var tcs = new TaskCompletionSource<object>();
			var registration = ThreadPool.RegisterWaitForSingleObject(handle, (state, timedOut) =>
			{
				var localTcs = (TaskCompletionSource<object>) state;
				if (timedOut)
					localTcs.TrySetCanceled();
				else
					localTcs.TrySetResult(null);
			}, tcs, timeout, true);
			tcs.Task.ContinueWith((_, state) => ((RegisteredWaitHandle) state).Unregister(null), registration,
				TaskScheduler.Default);
			return tcs.Task;
		}
	}

	public class SeriellTaskFactory
	{
		private Thread _thread;

		private volatile bool _working;

		public SeriellTaskFactory()
		{
			ConcurrentQueue = new ConcurrentQueue<ActionWaiter>();
		}

		private ConcurrentQueue<ActionWaiter> ConcurrentQueue { get; set; }

		public async Task AddAsync(Action action)
		{
			await Add(action).WaitHandle.AsTask();
		}

		public async Task<T> AddResultAsync<T>(Func<T> action)
		{
			var result = AddResult(action);
			await result.Waiter.WaitHandle.AsTask();
			return result.Result;
		}

		public ManualResetEventSlim Add(Action action)
		{
			var handler = new ActionWaiter(action);
			ConcurrentQueue.Enqueue(handler);
			StartScheduler();
			return handler.Waiter;
		}

		public WaiterResult<T> AddResult<T>(Func<T> action)
		{
			WaiterResult<T> result = null;
			var handler = new ActionWaiter(() => result.Result = action());
			result = new WaiterResult<T>(handler.Waiter);
			ConcurrentQueue.Enqueue(handler);
			StartScheduler();
			return result;
		}

		public void AddWait(Action action)
		{
			AddWaitAsync(action).Wait();
		}

		public async Task AddWaitAsync(Action action)
		{
			var handler = new ActionWaiter(action);
			ConcurrentQueue.Enqueue(handler);
			StartScheduler();
			await handler.Waiter.WaitHandle.AsTask();
		}


		public async Task<T> AddWaitAsync<T>(Func<T> action)
		{
			var value = default(T);
			var handler = new ActionWaiter(() => value = action());
			ConcurrentQueue.Enqueue(handler);
			StartScheduler();
			await handler.Waiter.WaitHandle.AsTask();
			return value;
		}

		public T AddWait<T>(Func<T> action)
		{
			return AddWaitAsync(action).Result;
		}

		private void StartScheduler()
		{
			if (_working)
				return;

			_working = true;
			_thread = new Thread(Worker);
			_thread.SetApartmentState(ApartmentState.MTA);
			_thread.Start();
		}

		internal void Worker()
		{
			while (ConcurrentQueue.Any())
			{
				ActionWaiter action;
				if (ConcurrentQueue.TryDequeue(out action))
					using (action.Waiter)
					{
						action.Action.Invoke();
						action.Waiter.Set();
					}
			}
			_working = false;
		}
	}
}