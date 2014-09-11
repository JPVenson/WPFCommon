using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace JPB.WPFBase.MVVM.ViewModel
{
    public abstract class AsyncViewModelBase : ThreadSaveViewModelBase
    {
        protected AsyncViewModelBase()
        {
            Factory = Task.Factory;
            Scheduler = Factory.Scheduler;
            CancellationToken = new CancellationToken(false);
        }

        #region IsWorking property

        private bool _isWorking = default(bool);

        public bool IsWorking
        {
            get { return _isWorking; }
            set
            {
                _isWorking = value;
                base.ThreadSaveAction(() =>
                {
                    SendPropertyChanged(() => IsWorking);
                    SendPropertyChanged(() => IsNotWorking);
                });
            }
        }

        #endregion

        #region IsNotWorking property

        public bool IsNotWorking
        {
            get { return !IsWorking; }
        }

        #endregion

        public CancellationToken CancellationToken
        {
            get { return _cancellationToken; }
            set
            {
                _cancellationToken = value;
            }
        }

        protected TaskScheduler Scheduler
        {
            get { return _scheduler; }
            set
            {
                if (value != null)
                {
                    _scheduler = value;
                    this.Factory = new TaskFactory(CancellationToken,TaskCreationOptions.PreferFairness,TaskContinuationOptions.None, value);
                }
            }
        }

        protected TaskFactory Factory
        {
            get { return _factory; }
            set
            {
                if (value != null) 
                    _factory = value;
            }
        }

        protected Task CurrentTask;
        private TaskScheduler _scheduler;
        private TaskFactory _factory;
        private CancellationToken _cancellationToken;

        public TaskAwaiter GetAwaiter()
        {
            if (IsWorking)
            {
                return CurrentTask.GetAwaiter();
            }
            return new TaskAwaiter();
        }

        private void StartWork()
        {
            IsWorking = true;
        }

        private void EndWork()
        {
            IsWorking = false;
        }

        public virtual bool OnTaskException(Exception exception)
        {
            return false;
        }

        protected void SimpleWorkWithSyncContinue<T>(Func<T> delegatetask, Action<T> continueWith, bool setWorking)
        {
            if (delegatetask != null)
            {
                SimpleWork(delegatetask, s => base.ThreadSaveAction(() => continueWith(s)), setWorking);
            }
        }

        public void SimpleWorkWithSyncContinue<T>(Func<T> delegatetask, Action<T> continueWith)
        {
            SimpleWorkWithSyncContinue(delegatetask, continueWith, true);
        }

        public void BackgroundSimpleWork(Action delegatetask)
        {
            if (delegatetask != null)
            {
                SimpleWork(delegatetask.Invoke, null, false);
            }
        }

        public void BackgroundSimpleWork<T>(Func<T> delegatetask, Action<T> continueWith)
        {
            if (delegatetask != null)
            {
                SimpleWork(delegatetask.Invoke, continueWith, false);
            }
        }

        public void SimpleWork(Action delegatetask)
        {
            if (delegatetask != null)
            {
                SimpleWork(delegatetask.Invoke);
            }
        }

        public void SimpleWork<T>(Func<T> delegatetask, Action<T> continueWith)
        {
            if (delegatetask != null)
            {
                SimpleWork(delegatetask.Invoke, continueWith);
            }
        }

        public void SimpleWork<T>(Func<T> task, Action<T> continueWith = null, bool setWOrking = true)
        {
            if (task != null)
            {
                if (setWOrking)
                    StartWork();
                var startNew = Factory.StartNew(task, TaskCreationOptions.PreferFairness);
                startNew.ContinueWith(s => CreateContinue(s, continueWith, setWOrking)());
                if (setWOrking)
                    CurrentTask = startNew;
            }
        }

        public void SimpleWork(Action task, Delegate continueWith = null, bool setWOrking = true)
        {
            if (task != null)
            {
                if (setWOrking)
                    StartWork();
                var startNew = Factory.StartNew(task, TaskCreationOptions.PreferFairness);
                startNew.ContinueWith(s => CreateContinue(s, continueWith, setWOrking)());
                if (setWOrking)
                    CurrentTask = startNew;
            }
        }

        //public void SimpleWork(Task task, Delegate continueWith, bool setWOrking = true)
        //{
        //    if (task != null)
        //    {
        //        if (setWOrking)
        //            StartWork();
        //        task.ContinueWith(s => CreateContinue(s, continueWith, setWOrking)());
        //        if (setWOrking)
        //            CurrentTask = task;
        //        task.Start();
        //    }
        //}

        private Action CreateContinue(Task s)
        {
            return CreateContinue(s, null);
        }

        private Action CreateContinue(Task s, bool setWorking)
        {
            return CreateContinue(s, null, setWorking);
        }

        private Action CreateContinue(Task s, Delegate continueWith, bool setWOrking = true)
        {
            Action contuine = () =>
            {
                try
                {
                    if (s.IsFaulted)
                    {
                        if (OnTaskException(s.Exception))
                            return;
                        if (s.Exception != null)
                            s.Exception.Handle(OnTaskException);
                    }
                    if (continueWith != null)
                        continueWith.DynamicInvoke(s);
                }
                finally
                {
                    if (setWOrking)
                        EndWork();
                }
            };

            return contuine;
        }

        public void BackgroundSimpleWork(Task task)
        {
            if (task != null)
                task.Start();
        }
    }
}