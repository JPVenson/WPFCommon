using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using System.Linq;

namespace JPB.WPFBase.MVVM.ViewModel
{
    public abstract class AsyncViewModelBase : ThreadSaveViewModelBase
    {
        #region IsWorking property

        private volatile bool _isWorking = default(bool);

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
                    CommandManager.InvalidateRequerySuggested();
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

        protected volatile Task CurrentTask;
        private const string AnonymousTask = "(A59E0AB1-AAC9-4774-A6C1-F13620AF5832)";

        protected AsyncViewModelBase(Dispatcher disp)
            : base(disp)
        {
            disp.ShutdownFinished += disp_ShutdownStarted;
            _namedTasks = new List<Tuple<string, Task>>();
        }

        void disp_ShutdownStarted(object sender, EventArgs e)
        {
            if (CurrentTask != null)
            {
                try
                {
                    CurrentTask.Wait(TimeSpan.FromMilliseconds(1));
                    if (CurrentTask.Status == TaskStatus.Running)
                    {
                        CurrentTask.Dispose();
                    }
                }
                catch (Exception)
                {
                    Trace.Write("Error due cleanup of tasks");
                }
            }
        }

        protected AsyncViewModelBase()
        {
            _namedTasks = new List<Tuple<string, Task>>();
        }

        public TaskAwaiter GetAwaiter()
        {
            if (IsWorking)
            {
                return CurrentTask.GetAwaiter();
            }
            return new TaskAwaiter();
        }

        /// <summary>
        /// Allows you to check for a Condtion if the calling method is named after the mehtod you would like to check but starts with "Can"
        /// </summary>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public bool CheckCanExecuteCondition([CallerMemberName]string taskName = AnonymousTask)
        {
            if (taskName == AnonymousTask)
            {
                return this[taskName];
            }

            if (!taskName.StartsWith("Can"))
            {
                return this[taskName];
            }

            return this[taskName.Remove(0, 3)];
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

        private readonly List<Tuple<string, Task>> _namedTasks;

        protected bool this[string index]
        {
            get { return _namedTasks.All(s => s.Item1 != index); }
        }

        protected void SimpleWorkWithSyncContinue<T>(Func<T> delegatetask, Action<T> continueWith, bool setWorking, [CallerMemberName]string taskName = AnonymousTask)
        {
            if (delegatetask != null)
            {
                var task = new Task<T>(delegatetask.Invoke);
                SimpleWorkInternal(task, new Action<Task<T>>(s =>
                    base.ThreadSaveAction(() => continueWith(s.Result))), taskName, setWorking);
            }
        }

        public void SimpleWorkWithSyncContinue<T>(Func<T> delegatetask, Action<T> continueWith, [CallerMemberName]string taskName = AnonymousTask)
        {
            if (delegatetask != null)
            {
                var task = new Task<T>(delegatetask.Invoke);
                SimpleWorkInternal(task, new Action<Task<T>>(s =>
                    base.ThreadSaveAction(() => continueWith(s.Result))), taskName, true);
            }
        }

        public void BackgroundSimpleWork(Action delegatetask, [CallerMemberName]string taskName = AnonymousTask)
        {
            if (delegatetask != null)
            {
                var task = new Task(delegatetask.Invoke);
                SimpleWorkInternal(task, null, taskName, false);
            }
        }

        public void BackgroundSimpleWork<T>(Func<T> delegatetask, Action<T> continueWith, [CallerMemberName]string taskName = AnonymousTask)
        {
            if (delegatetask != null)
            {
                var task = new Task<T>(delegatetask.Invoke);
                SimpleWorkInternal(task, continueWith, taskName, false);
            }
        }

        public void SimpleWork(Action delegatetask, [CallerMemberName]string taskName = AnonymousTask)
        {
            if (delegatetask != null)
            {
                var task = new Task(delegatetask.Invoke);
                SimpleWorkInternal(task, null, taskName, true);
            }
        }

        public void SimpleWork<T>(Func<T> delegatetask, Action<T> continueWith, [CallerMemberName]string taskName = AnonymousTask)
        {
            if (delegatetask != null)
            {
                var task = new Task<T>(delegatetask.Invoke);
                SimpleWorkInternal(task, new Action<Task<T>>(s => continueWith(s.Result)), taskName, true);
            }
        }

        public void SimpleWork(Delegate delegatetask, Delegate continueWith, [CallerMemberName]string taskName = AnonymousTask)
        {
            if (delegatetask != null)
            {
                var task = new Task(() => delegatetask.DynamicInvoke());
                SimpleWorkInternal(task, continueWith);
            }
        }

        public void SimpleWork(Delegate delegatetask, [CallerMemberName]string taskName = AnonymousTask)
        {
            SimpleWorkInternal(new Task(() => delegatetask.DynamicInvoke()), null, taskName, true);
        }

        public void SimpleWork(Task task, Delegate continueWith, [CallerMemberName]string taskName = AnonymousTask, bool setWorking = true)
        {
            SimpleWorkInternal(task, continueWith, taskName, setWorking);
        }

        private void SimpleWorkInternal(Task task, Delegate continueWith, string taskName = AnonymousTask, bool setWorking = true)
        {
            if (task != null)
            {
                _namedTasks.Add(new Tuple<string, Task>(taskName, task));
                if (setWorking)
                    StartWork();
                task.ContinueWith(s => CreateContinue(s, continueWith, taskName, setWorking)());
                if (setWorking)
                    CurrentTask = task;
                BackgroundSimpleWork(task);
            }
        }

        private Action CreateContinue(Task s, bool setWorking, string taskName)
        {
            return CreateContinue(s, null, taskName, setWorking);
        }

        private Action CreateContinue(Task s, Delegate continueWith, string taskName, bool setWOrking = true)
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
                    var fod = _namedTasks.FirstOrDefault(e => e.Item1 == taskName);
                    this._namedTasks.Remove(fod);
                }
            };

            return contuine;
        }

        public void BackgroundSimpleWork(Task task)
        {
            if (task != null)
                task.Start();
        }

        public void SimpleWork(Task task, [CallerMemberName]string taskName = AnonymousTask)
        {
            SimpleWorkInternal(task, null, taskName, true);
        }

        public void SimpleWork(Task task, bool setWorking, [CallerMemberName]string taskName = AnonymousTask)
        {
            SimpleWorkInternal(task, null, taskName, setWorking);
        }
    }
}