using System;
using System.Windows;
using System.Windows.Threading;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Dispatcher = Windows.UI.Core.CoreDispatcher;
#endif

namespace JPB.WPFBase.MVVM.ViewModel
{
    public abstract class ThreadSaveViewModelActor
    {
        protected ThreadSaveViewModelActor()
#if WINDOWS_UWP
			: this(Window.Current.Dispatcher)
#else
            : this(Application.Current.Dispatcher)
#endif
        {
        }

        protected ThreadSaveViewModelActor(Dispatcher targetDispatcher)
        {
            Dispatcher = targetDispatcher ??
#if WINDOWS_UWP
			Window.Current.Dispatcher
#else
            Application.Current.Dispatcher
#endif
                ;
            Lock = new object();
        }

        protected Dispatcher Dispatcher { get; set; }

        public object Lock { get; set; }

        public bool IsLocked { get; protected set; }

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