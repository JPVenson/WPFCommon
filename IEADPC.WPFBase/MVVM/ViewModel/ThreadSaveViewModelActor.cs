using System;
using System.Windows;
using System.Windows.Threading;

namespace WPFBase.MVVM.ViewModel
{
    public abstract class ThreadSaveViewModelActor
    {
        protected ThreadSaveViewModelActor()
        {
            Dispatcher = Application.Current.Dispatcher;
            Lock = new object();
        }

        protected ThreadSaveViewModelActor(Dispatcher targetDispatcher)
        {
            Dispatcher = targetDispatcher;
        }

        protected Dispatcher Dispatcher { get; set; }

        public object Lock { get; set; }
        public bool IsLocked { get; set; }

        public void ThreadSaveAction(Action action)
        {
            try
            {
                IsLocked = true;
                if (Dispatcher.CheckAccess())
                    action();
                else
                    Dispatcher.Invoke(action, DispatcherPriority.DataBind);
            }
            finally
            {
                IsLocked = false;
            }
        }

        public void BeginThreadSaveAction(Action action)
        {
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
                Dispatcher.BeginInvoke(action, DispatcherPriority.DataBind);
        }
    }
}