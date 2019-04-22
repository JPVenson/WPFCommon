using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using JetBrains.Annotations;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Dispatcher = Windows.UI.Core.CoreDispatcher;
#endif

namespace JPB.WPFBase.MVVM.ViewModel
{
	/// <summary>
	///		A Base class for Thread save Actions. All actions that are "thread-save" are executed with the Dispatcher given in the constructor.
	///		If the empty constructor is used the current dispatcher from <seealso cref="DispatcherLock"/> is taken
	/// </summary>
	public abstract class ThreadSaveViewModelActor
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ThreadSaveViewModelActor"/> class.
		/// </summary>
		/// <param name="targetDispatcher">The target dispatcher.</param>
		protected ThreadSaveViewModelActor(Dispatcher targetDispatcher = null)
		{
			Dispatcher = targetDispatcher ?? DispatcherLock.GetDispatcher();
			Lock = new object();
		}

		/// <summary>
		/// Shorthand for <code>DispatcherLock.CatpureDispatcher()</code>
		/// </summary>
		/// <returns></returns>
		protected IDisposable Catch()
		{
			return DispatcherLock.CaptureDispatcher();
		}

		/// <summary>
		///		Gets the Dispatcher that are used for <seealso cref="ThreadSaveAction"/> and <seealso cref="BeginThreadSaveAction"/>
		/// </summary>
		[ProvidesContext]
		protected Dispatcher Dispatcher { get; set; }

		/// <summary>
		///		The Internal LockRoot for async operations
		/// </summary>
		[ProvidesContext]
		public object Lock { get; set; }

		/// <summary>
		///		Indicates a Running operation in the Dispatcher.
		/// </summary>
		[ProvidesContext]
		protected bool IsLocked { get; set; }

		/// <summary>
		///		Dispatches an Action into the Dispatcher.
		///		If this method is called from the dispatcher the action will be executed in this thread.
		///		The action will be executed with <seealso cref="DispatcherPriority.DataBind"/>
		/// </summary>
		/// <param name="action">The action.</param>
		public void ThreadSaveAction(Action action)
		{
			try
			{
				if (Dispatcher.HasShutdownStarted)
				{
					return;
				}

				IsLocked = true;
				if (Dispatcher.CheckAccess())
				{
					action();
				}
				else
				{
					Dispatcher.Invoke(action, DispatcherPriority.DataBind);
				}
			}
			finally
			{
				IsLocked = false;
			}
		}

		/// <summary>
		///		Dispatches an Action into the Dispatcher and continues.
		///		If this method is called from the dispatcher the action will be executed in this thread.
		///		The action will be executed with <seealso cref="DispatcherPriority.DataBind"/>
		/// </summary>
		/// <param name="action">The action.</param>
		public DispatcherOperationLite BeginThreadSaveAction(Action action)
		{
			if (Dispatcher.HasShutdownStarted)
			{
				return null;
			}

			if (!Dispatcher.CheckAccess())
			{
				return new DispatcherOperationLite(Dispatcher.BeginInvoke(action, DispatcherPriority.DataBind));
			}

			try
			{
				IsLocked = true;
				action();
				return new DispatcherOperationLite(Dispatcher, DispatcherPriority.DataBind,
					DispatcherOperationStatus.Completed);
			}
			finally
			{
				IsLocked = false;
			}
		}
	}

	/// <summary>
	///		The wrapper for <seealso cref="System.Windows.Threading.DispatcherOperation"/>
	/// </summary>
	public class DispatcherOperationLite
	{
		private readonly DispatcherOperation _operation;
		private readonly Dispatcher _dispatcher;
		private readonly DispatcherPriority _priority;
		private readonly DispatcherOperationStatus _status;

		/// <summary>
		/// Initializes a new instance of the <see cref="DispatcherOperationLite"/> class.
		/// </summary>
		/// <param name="operation">The operation.</param>
		public DispatcherOperationLite(DispatcherOperation operation)
		{
			_operation = operation;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DispatcherOperationLite"/> class.
		/// </summary>
		/// <param name="dispatcher">The dispatcher.</param>
		/// <param name="priority">The priority.</param>
		/// <param name="status">The status.</param>
		public DispatcherOperationLite(Dispatcher dispatcher, DispatcherPriority priority, DispatcherOperationStatus status)
		{
			_dispatcher = dispatcher;
			_priority = priority;
			_status = status;
		}

		/// <summary>Gets the <see cref="T:System.Windows.Threading.Dispatcher" /> that the operation was posted to. </summary>
		/// <returns>The dispatcher.</returns>
		public Dispatcher Dispatcher
		{
			get { return _operation?.Dispatcher ?? _dispatcher; }
		}

		/// <summary>Gets or sets the priority of the operation in the <see cref="T:System.Windows.Threading.Dispatcher" /> queue. </summary>
		/// <returns>The priority of the delegate on the queue.</returns>
		public DispatcherPriority Priority
		{
			get { return _operation?.Priority ?? _priority; }
		}

		/// <summary>Gets the current status of the operation..</summary>
		/// <returns>The status of the operation.</returns>
		public DispatcherOperationStatus Status
		{
			get { return _operation?.Status ?? _status; }
		}

		/// <summary>
		/// Gets the task.
		/// </summary>
		/// <returns></returns>
		public Task GetTask()
		{
			return _operation.Task ?? Task.CompletedTask;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool Abort()
		{
			return _operation?.Abort() ?? false;
		}
	}
}