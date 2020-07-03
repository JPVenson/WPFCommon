using System;
using System.Windows.Threading;
using JetBrains.Annotations;

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
		[PublicAPI]
		[MustUseReturnValue]
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
		public void ThreadSaveAction([NotNull]Action action)
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
		[CanBeNull]
		[PublicAPI]
		public DispatcherOperationLite BeginThreadSaveAction([NotNull]Action action)
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
}