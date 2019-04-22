using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using JPB.Tasking.TaskManagement.Threading;

namespace JPB.WPFBase.Logger
{
	public class DispatcherStatusMonitor
	{
		private readonly Dispatcher _dispatcher;
		private readonly EventDispatcherLimited _actionDispatcher; 

		public DispatcherStatusMonitor(Dispatcher dispatcher, Action<DispatcherStatusOperationMessage> callback)
		{
			_dispatcher = dispatcher;
			_actionDispatcher = new EventDispatcherLimited(200, callback);
		}

		private class EventDispatcherLimited
		{
			private readonly Action<DispatcherStatusOperationMessage> _callback;

			public EventDispatcherLimited(int limit, Action<DispatcherStatusOperationMessage> callback)
			{
				_callback = callback;
				Limit = limit;
				Messages = new BlockingCollection<DispatcherStatusOperationMessage>();
			}

			public int Limit { get; }

			private volatile int _messagesOmited;
			private BlockingCollection<DispatcherStatusOperationMessage> Messages { get; }

			public void Add(DispatcherStatusOperationMessage action)
			{
				if (Messages.Count > Limit)
				{
					Interlocked.Add(ref _messagesOmited, 1);
					return;
				}

				Messages.Add(action);
			}

			public void Start()
			{
				Task.Run(() =>
				{
					foreach (var dispatcherStatusOperation in Messages.GetConsumingEnumerable())
					{
						_callback(dispatcherStatusOperation);
					}
				});
			}
		}

		public struct DispatcherStatusOperationMessage
		{
			public DispatcherStatusOperationMessage( object message)
			{
				Time = DateTime.Now;
				Message = message;
			}
			public DateTime Time { get; private set; }
			public object Message { get; private set; }
		}

		private CancellationTokenSource _stopRequested;

		public void Start()
		{
			_stopRequested = new CancellationTokenSource();
			_dispatcherOperations = new ConcurrentDictionary<DispatcherOperation, List<DispatcherOperationStatusMessage>>();
			_dispatcher.Hooks.DispatcherInactive += Hooks_DispatcherInactive;
			_dispatcher.Hooks.OperationPosted += Hooks_OperationPosted;
			_dispatcher.Hooks.OperationStarted += Hooks_OperationStarted;
			_dispatcher.Hooks.OperationCompleted += HooksOnOperationCompleted;
			_dispatcher.Hooks.OperationAborted += Hooks_OperationAborted;
			_actionDispatcher.Start();

			Task.Run(() =>
			{
				while (!_stopRequested.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(1)))
				{
					var operationsSnapshot = _dispatcherOperations.ToArray();
					if (operationsSnapshot.Length == 0)
					{
						continue;
					}

					foreach (var keyValuePair in 
						operationsSnapshot.Where(e => e.Key.Status == DispatcherOperationStatus.Aborted
						                              || e.Key.Status == DispatcherOperationStatus.Completed))
					{
						_dispatcherOperations.TryRemove(keyValuePair.Key, out _);
					}

					Func<DispatcherOperationStatusMessage, DateTime> selector = f => f.Time;
					var longestOperation = operationsSnapshot
						.Where(e => e.Key.Status == DispatcherOperationStatus.Completed)
						.OrderBy(e => e.Value.Max(selector) - e.Value.Max(selector))
						.FirstOrDefault();

					var text = $"In the last second the dispatcher has " +
					           $"completed '{operationsSnapshot.Count(f => f.Key.Status == DispatcherOperationStatus.Completed)}' " +
					           $"posted '{operationsSnapshot.Count(f => f.Key.Status == DispatcherOperationStatus.Pending)}' " +
					           $"Aborted '{operationsSnapshot.Count(f => f.Key.Status == DispatcherOperationStatus.Aborted)}' ";
					if (longestOperation.Key != null)
					{
						text +=
							$"The longest operation took " +
							$"'{(longestOperation.Value.Max(selector) - longestOperation.Value.Min(selector)).ToString("c")}'" +
							$"and was of priority '{longestOperation.Key.Priority}' ";
					}
					var operationDescription = new DispatcherStatusOperationMessage(text);
					_actionDispatcher.Add(operationDescription);
				}
			});
		}

		private void Hooks_OperationAborted(object sender, DispatcherHookEventArgs e)
		{
			AddOperation(e.Operation);
		}

		private void HooksOnOperationCompleted(object sender, DispatcherHookEventArgs e)
		{
			AddOperation(e.Operation);
		}

		private void Hooks_OperationStarted(object sender, DispatcherHookEventArgs e)
		{
			AddOperation(e.Operation);
		}

		private void Hooks_OperationPosted(object sender, DispatcherHookEventArgs e)
		{
			AddOperation(e.Operation);
		}

		private void AddOperation(DispatcherOperation operation)
		{
			if (!_dispatcherOperations.TryGetValue(operation, out var listOfOperations))
			{
				listOfOperations = new List<DispatcherOperationStatusMessage>();
				_dispatcherOperations.TryAdd(operation, listOfOperations);
			}
			listOfOperations.Add(new DispatcherOperationStatusMessage(operation, operation.Status));

		}

		public struct DispatcherOperationStatusMessage
		{
			public DispatcherOperationStatusMessage(DispatcherOperation operation, DispatcherOperationStatus type)
			{
				Operation = operation;
				Type = type;
				Time = DateTime.Now;
			}

			public DispatcherOperation Operation { get; private set; }
			public DateTime Time { get; private set; }
			public DispatcherOperationStatus Type { get; private set; }
		}

		private ConcurrentDictionary<DispatcherOperation, List<DispatcherOperationStatusMessage>> _dispatcherOperations;

		private void Hooks_DispatcherInactive(object sender, EventArgs e)
		{
		}
	}
}
