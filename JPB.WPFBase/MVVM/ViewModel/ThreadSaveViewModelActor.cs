using System;
using System.Windows;
using System.Windows.Threading;

namespace JPB.WPFBase.MVVM.ViewModel
{
    public abstract class ThreadSaveViewModelActor
    {
        protected ThreadSaveViewModelActor()
            : this(Application.Current.Dispatcher)
        {
            
        }

        protected ThreadSaveViewModelActor(Dispatcher targetDispatcher)
        {
            Dispatcher = targetDispatcher ?? Application.Current.Dispatcher;
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