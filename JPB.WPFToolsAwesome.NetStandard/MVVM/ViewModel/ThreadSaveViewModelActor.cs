using System;
using System.Windows.Threading;


#if WINDOWS_UWP
using Windows.UI.Xaml;
using Dispatcher = Windows.UI.Core.CoreDispatcher;
#endif

namespace JPB.WPFToolsAwesome.MVVM.ViewModel
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
		///		Gets the Dispatcher that are used for <seealso cref="ViewModelAction(System.Action)"/> and <seealso cref="BeginViewModelAction(System.Action)"/>
		/// </summary>
		protected Dispatcher Dispatcher { get; set; }

		/// <summary>
		///		The Internal LockRoot for async operations
		/// </summary>
		public object Lock { get; set; }

		/// <summary>
		///		Indicates a Running operation in the Dispatcher.
		/// </summary>
		protected bool IsLocked { get; set; }

		///  <summary>
		/// 		Dispatches an Action into the Dispatcher.
		/// 		If this method is called from the dispatcher the action will be executed in this thread.
		/// 		The action will be executed with <seealso cref="DispatcherPriority.DataBind"/>
		///  </summary>
		///  <param name="action">The action.</param>
		///  <param name="priority">The Dispatcher Priority. Defaults to <see cref="DispatcherPriority.DataBind"/></param>
		public void ViewModelAction(Action action, DispatcherPriority priority = DispatcherPriority.DataBind)
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
					Dispatcher.Invoke(action, priority);
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
		///  <param name="priority">The Dispatcher Priority. Defaults to <see cref="DispatcherPriority.DataBind"/></param>
		public DispatcherOperationLite BeginViewModelAction(Action action,
			DispatcherPriority priority = DispatcherPriority.DataBind)
		{
			if (Dispatcher.HasShutdownStarted)
			{
				return null;
			}

			if (!Dispatcher.CheckAccess())
			{
				return new DispatcherOperationLite(Dispatcher.BeginInvoke(action, priority));
			}

			try
			{
				IsLocked = true;
				action();
				return new DispatcherOperationLite(Dispatcher, 
					priority,
					DispatcherOperationStatus.Completed);
			}
			finally
			{
				IsLocked = false;
			}
		}
		
		/// <summary>
		///		Dispatches an Action into the Dispatcher.
		///		If this method is called from the dispatcher the action will be executed in this thread.
		///		The action will be executed with <seealso cref="DispatcherPriority.DataBind"/>
		/// </summary>
		/// <param name="action">The action.</param>
		[Obsolete("Use ViewModelAction")]
		public void ThreadSaveAction(Action action)
		{
			ViewModelAction(action);
		}

		/// <summary>
		///		Dispatches an Action into the Dispatcher and continues.
		///		If this method is called from the dispatcher the action will be executed in this thread.
		///		The action will be executed with <seealso cref="DispatcherPriority.DataBind"/>
		/// </summary>
		/// <param name="action">The action.</param>
		[Obsolete("Use BeginViewModelAction")]
		public DispatcherOperationLite BeginThreadSaveAction(Action action)
		{
			return BeginViewModelAction(action);
		}
	}
}