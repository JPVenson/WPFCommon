﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace JPB.WPFToolsAwesome.Logger.DispatcherLogger
{
	/// <summary>
	///		Can observe several status infos from the dispatcher
	/// </summary>
	public class DispatcherStatusMonitor : IDisposable
	{
		private readonly Dispatcher _dispatcher;
		private readonly EventDispatcherLimited _actionDispatcher;

		/// <summary>
		///		Creates a new Dispatcher Status Monitor
		/// </summary>
		/// <param name="dispatcher"></param>
		/// <param name="callback"></param>
		public DispatcherStatusMonitor(Dispatcher dispatcher, Action<IDispatcherFrameInfo> callback)
		{
			_dispatcher = dispatcher;
			_actionDispatcher = new EventDispatcherLimited(200, callback);
		}

		private class EventDispatcherLimited
		{
			private readonly Action<IDispatcherFrameInfo> _callback;

			public EventDispatcherLimited(int limit, Action<IDispatcherFrameInfo> callback)
			{
				_callback = callback;
				Limit = limit;
				Messages = new BlockingCollection<IDispatcherFrameInfo>();
			}

			public int Limit { get; }

			private volatile int _messagesOmited;
			private BlockingCollection<IDispatcherFrameInfo> Messages { get; }

			public void Add(IDispatcherFrameInfo action)
			{
				if (Messages.Count > Limit)
				{
					Interlocked.Add(ref _messagesOmited, 1);
					return;
				}

				if (Messages.IsAddingCompleted)
				{
					return;
				}

				Messages.Add(action);
			}

			public void Start()
			{
				Task.Factory.StartNew(() =>
				{
					foreach (var dispatcherStatusOperation in Messages.GetConsumingEnumerable())
					{
						_callback(dispatcherStatusOperation);
					}
				}, TaskCreationOptions.LongRunning);
			}

			public void Stop()
			{
				Messages.CompleteAdding();
			}
		}

		/// <summary>
		///		Infos about a status information
		/// </summary>
		public interface IDispatcherFrameInfo
		{
			/// <summary>
			///		When does this action occured
			/// </summary>
			DateTime Time { get; }
		}

		/// <summary>
		///		Is periodically raised
		/// </summary>
		public class DispatcherFrameInfo : IDispatcherFrameInfo
		{
			/// <summary>
			///		The snapshot of all known operations at this time
			/// </summary>
			public KeyValuePair<DispatcherOperation, DispatcherOperationStatusMessage[]>[] OperationsSnapshot { get; }

			/// <param name="operationsSnapshot"></param>
			/// <inheritdoc />
			public DispatcherFrameInfo(
				KeyValuePair<DispatcherOperation, DispatcherOperationStatusMessage[]>[] operationsSnapshot)
			{
				OperationsSnapshot = operationsSnapshot;
				Time = DateTime.Now;
			}

			/// <inheritdoc />
			public DateTime Time { get; }

			/// <summary>
			///		How many Operations are currently Executing. A Value bigger then 1 indicates async running dispatcher Operations
			/// </summary>
			public int Executing
			{
				get { return OperationsSnapshot.Count(f => f.Key.Status == DispatcherOperationStatus.Executing); }
			}

			/// <summary>
			///		How many Operations where completed in comparison to a previous Snapshot
			/// </summary>
			public int Completed
			{
				get { return OperationsSnapshot.Count(f => f.Key.Status == DispatcherOperationStatus.Completed); }
			}

			/// <summary>
			///		How many Operations are pending in comparison to a previous Snapshot
			/// </summary>
			public int Pending
			{
				get { return OperationsSnapshot.Count(f => f.Key.Status == DispatcherOperationStatus.Pending); }
			}

			/// <summary>
			///		How many Operations are Aborted in comparison to a previous Snapshot
			/// </summary>
			public int Aborted
			{
				get { return OperationsSnapshot.Count(f => f.Key.Status == DispatcherOperationStatus.Aborted); }
			}

			/// <summary>
			///		Gets the longest running operation in this Snapshot
			/// </summary>
			public Tuple<DispatcherOperation, TimeSpan> LongestOperation
			{
				get
				{
					Func<DispatcherOperationStatusMessage, DateTime> selector = f => f.Time;
					var longestOperation = OperationsSnapshot
						.Where(e => e.Key.Status == DispatcherOperationStatus.Completed)
						.OrderBy(e => e.Value.Max(selector) - e.Value.Max(selector))
						.FirstOrDefault();
					return new Tuple<DispatcherOperation, TimeSpan>(longestOperation.Key, longestOperation.Value.Max(selector) - longestOperation.Value.Min(selector));
				}
			}
		}

		private CancellationTokenSource _stopRequested;

		///  <summary>
		/// 		Stops any currently running dispatcher operation if any is running
		///  </summary>
		///  <param name="waitForLastCircle">If set the stop method will wait for at least one circle of reporting the status operations</param>
		public void Stop(bool waitForLastCircle)
		{
			if (waitForLastCircle)
			{
				throw new NotImplementedException("The WaitForLastCircle flag is reserved for future use.");
			}

			_stopRequested.Cancel();
			_stopRequested = null;
			_dispatcher.Hooks.DispatcherInactive -= Hooks_DispatcherInactive;
			_dispatcher.Hooks.OperationPosted -= Hooks_OperationPosted;
			_dispatcher.Hooks.OperationStarted -= Hooks_OperationStarted;
			_dispatcher.Hooks.OperationCompleted -= HooksOnOperationCompleted;
			_dispatcher.Hooks.OperationAborted -= Hooks_OperationAborted;
			_actionDispatcher.Stop();
			_dispatcherOperations.Clear();
		}

		/// <summary>
		///		Starts observing the current dispatcher. If not otherwise specified, the frame that is observed is one second
		/// </summary>
		public void Start(TimeSpan? observerFrame = null)
		{
			var absolutObserverFrame = observerFrame ?? TimeSpan.FromSeconds(1);
			if (absolutObserverFrame.Ticks < 1)
			{
				throw new ArgumentException("The observerFrame cannot be less then 1 tick", "observerFrame");
			}

			_stopRequested = new CancellationTokenSource();
			_dispatcherOperations = new ConcurrentDictionary<DispatcherOperation, DispatcherOperationStatusMessage[]>();
			_dispatcher.Hooks.DispatcherInactive += Hooks_DispatcherInactive;
			_dispatcher.Hooks.OperationPosted += Hooks_OperationPosted;
			_dispatcher.Hooks.OperationStarted += Hooks_OperationStarted;
			_dispatcher.Hooks.OperationCompleted += HooksOnOperationCompleted;
			_dispatcher.Hooks.OperationAborted += Hooks_OperationAborted;
			_actionDispatcher.Start();

			Task.Factory.StartNew(() =>
			{
				while (!_stopRequested.Token.WaitHandle.WaitOne(absolutObserverFrame))
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
					_actionDispatcher.Add(new DispatcherFrameInfo(operationsSnapshot));
				}
			}, TaskCreationOptions.LongRunning);
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
				listOfOperations = new DispatcherOperationStatusMessage[4];
				_dispatcherOperations.TryAdd(operation, listOfOperations);
			}
			listOfOperations[(int)operation.Status] = new DispatcherOperationStatusMessage(operation, operation.Status);
		}

		/// <summary>
		///		A single message published by the ActionDispatcher
		/// </summary>
		public struct DispatcherOperationStatusMessage
		{
			/// <summary>
			/// 
			/// </summary>
			/// <param name="operation"></param>
			/// <param name="type"></param>
			public DispatcherOperationStatusMessage(DispatcherOperation operation, DispatcherOperationStatus type)
			{
				Operation = operation;
				Type = type;
				Time = DateTime.Now;
			}

			/// <summary>
			///		The Operation Recorded
			/// </summary>
			public DispatcherOperation Operation { get; private set; }

			/// <summary>
			///		When was this message Recorded
			/// </summary>
			public DateTime Time { get; private set; }

			/// <summary>
			///		What status was recorded
			/// </summary>
			public DispatcherOperationStatus Type { get; private set; }
		}

		private ConcurrentDictionary<DispatcherOperation, DispatcherOperationStatusMessage[]> _dispatcherOperations;

		private void Hooks_DispatcherInactive(object sender, EventArgs e)
		{
		}

		public void Dispose()
		{
			_stopRequested?.Cancel();
		}
	}
}
