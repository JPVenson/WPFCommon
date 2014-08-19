﻿using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace JPB.WPFBase.MVVM.ViewModel
{
    public abstract class AsyncViewModelBase : ThreadSaveViewModelBase
    {
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

        protected Task CurrentTask;

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
                var task = new Task<T>(delegatetask.Invoke);
                SimpleWork(task, new Action<Task<T>>(s =>
                    base.ThreadSaveAction(() => continueWith(s.Result))), setWorking);
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
                var task = new Task(delegatetask.Invoke);
                SimpleWork(task, false);
            }
        }

        public void BackgroundSimpleWork<T>(Func<T> delegatetask, Action<T> continueWith)
        {
            if (delegatetask != null)
            {
                var task = new Task<T>(delegatetask.Invoke);
                SimpleWork(task, continueWith, false);
            }
        }

        public void SimpleWork(Action delegatetask)
        {
            if (delegatetask != null)
            {
                var task = new Task(delegatetask.Invoke);
                SimpleWork(task);
            }
        }

        public void SimpleWork<T>(Func<T> delegatetask, Action<T> continueWith)
        {
            if (delegatetask != null)
            {
                var task = new Task<T>(delegatetask.Invoke);
                SimpleWork(task, new Action<Task<T>>(s => continueWith(s.Result)));
            }
        }

        public void SimpleWork(Delegate delegatetask, Delegate continueWith)
        {
            if (delegatetask != null)
            {
                var task = new Task(() => delegatetask.DynamicInvoke());
                SimpleWork(task, continueWith);
            }
        }

        public void SimpleWork(Delegate delegatetask)
        {
            SimpleWork(new Task(() => delegatetask.DynamicInvoke()));
        }

        public void SimpleWork(Task task, Delegate continueWith, bool setWOrking = true)
        {
            if (task != null)
            {
                if (setWOrking)
                    StartWork();
                task.ContinueWith(s => CreateContinue(s, continueWith, setWOrking)());
                if (setWOrking)
                    CurrentTask = task;
                task.Start();
            }
        }

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

        public void SimpleWork(Task task)
        {
            SimpleWork(task, true);
        }

        public void SimpleWork(Task task, bool setWorking)
        {
            SimpleWork(task, null, setWorking);
        }
    }
}