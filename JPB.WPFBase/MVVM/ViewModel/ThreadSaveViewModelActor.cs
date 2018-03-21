using System;
using System.Windows;
using System.Windows.Threading;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Dispatcher = Windows.UI.Core.CoreDispatcher;
#endif

namespace JPB.WPFBase.MVVM.ViewModel
{
	/// <summary>
	/// Base class for all UI related actors
	/// </summary>
	public abstract class ThreadSaveViewModelActor
    {
		/// <summary>
		/// Creates a new <code>ThreadSaveViewModelActor</code> and attaches the current dispatcher.
		/// Will create no exception if the current Thread is not an UI Thread.
		/// You can capture the Dispatcher Scope by using the <code>Catch</code> method
		/// </summary>
	    protected ThreadSaveViewModelActor()
			    : this(DispatcherLock.GetDispatcher())
	    {
	    }

		/// <summary>
		/// Creates a new <code>ThreadSaveViewModelActor</code> and attaches the given dispatcher
		/// </summary>
		/// <param name="targetDispatcher"></param>
	    protected ThreadSaveViewModelActor(Dispatcher targetDispatcher)
	    {
		    this.Dispatcher = targetDispatcher;
		    this.Lock = new object();
	    }

		/// <summary>
		/// Captures the current Dispatcher Scope.
		/// </summary>
		/// <returns></returns>
	    protected IDisposable CatpureScope()
	    {
		    return DispatcherLock.CatpureDispatcher();
	    }

		/// <summary>
		///		The Related Dispatcher attached to this <code>ThreadSaveViewModelActor</code>
		/// </summary>
		protected Dispatcher Dispatcher { get; set; }

		/// <summary>
		///		A Instance related Lock object
		/// </summary>
        public object Lock { get; set; }

		/// <summary>
		///		An informational Locked flag. If this Flag is set another thread or the current Thread is currently accessing the
		///		given dispatcher.
		/// </summary>
        public bool IsLocked { get; protected set; }

		/// <summary>
		///		Executes the given <paramref name="action"/> inside the given <code>Dispatcher</code>.
		///		If the caller dispatcher IS the given <code>Dispatcher</code>, it will execute synchrony
		/// </summary>
		/// <param name="action"></param>
		public void ThreadSaveAction(Action action)
        {
            try
            {
                if (Dispatcher.HasShutdownStarted)
                    return;

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
		///		Executes the given <paramref name="action"/> inside the given <code>Dispatcher</code> by using BeginInvoke.
		///		If the caller dispatcher IS the given <code>Dispatcher</code>, it will execute synchrony
		/// </summary>
		/// <param name="action"></param>
		public void BeginThreadSaveAction(Action action)
        {
            if (Dispatcher.HasShutdownStarted)
                return;

            if (Dispatcher.CheckAccess())
            {
                try
                {
                    IsLocked = true;
                    action();
                }
                finally
                {
                    IsLocked = false;
                }
            }
            else
            {
                Dispatcher.BeginInvoke(action, DispatcherPriority.DataBind);
            }
        }
    }
}