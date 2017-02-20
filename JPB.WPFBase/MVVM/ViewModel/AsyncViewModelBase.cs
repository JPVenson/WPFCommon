using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace JPB.WPFBase.MVVM.ViewModel
{
    public abstract class AsyncViewModelBase : ViewModelBase
    {
        private const string AnonymousTask = "AnonymousTask";

        private readonly List<Tuple<string, Task>> _namedTasks;

        protected volatile Task CurrentTask;

        protected AsyncViewModelBase(Dispatcher disp)
            : base(disp)
        {
            disp.ShutdownFinished += disp_ShutdownStarted;
            _namedTasks = new List<Tuple<string, Task>>();
        }

        protected AsyncViewModelBase()
        {
            _namedTasks = new List<Tuple<string, Task>>();
        }

        #region IsNotWorking property

        public bool IsNotWorking
        {
            get { return !IsWorking; }
        }

        #endregion

        protected bool this[string index]
        {
            get { return _namedTasks.All(s => s.Item1 != index); }
        }

        private void disp_ShutdownStarted(object sender, EventArgs e)
        {
            if (CurrentTask != null)
                try
                {
                    foreach (var namedTask in _namedTasks)
                    {
                        if (namedTask.Item2.Status == TaskStatus.Running)
                        {

#if !WINDOWS_UWP
                            namedTask.Item2.Dispose();
#endif
                        }
                    }

                    //                    CurrentTask.Wait(TimeSpan.FromMilliseconds(1));
                    //                    if (CurrentTask.Status == TaskStatus.Running)
                    //                    {
                    //#if !WINDOWS_UWP
                    //                        CurrentTask.Dispose();
                    //#endif
                    //                    }
                }
                catch (Exception)
                {
#if !WINDOWS_UWP
                    Trace.Write("Error due cleanup of tasks");
#endif
                }
        }

        public TaskAwaiter GetAwaiter()
        {
            if (IsWorking)
                return CurrentTask.GetAwaiter();
            return new TaskAwaiter();
        }

        /// <summary>
        ///     Allows you to check for a Condtion if the calling method is named after the mehtod you would like to check but
        ///     starts with "Can"
        /// </summary>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public bool CheckCanExecuteCondition([CallerMemberName] string taskName = AnonymousTask)
        {
            if (taskName == AnonymousTask)
                return this[taskName];

            if (!taskName.StartsWith("Can"))
                return this[taskName];

            return this[taskName.Remove(0, 3)];
        }


        protected virtual void StartWork()
        {
            IsWorking = true;
        }

        protected virtual void EndWork()
        {
            IsWorking = _namedTasks.Any();
        }

        protected virtual bool OnTaskException(Exception exception)
        {
            return false;
        }

        protected Task SimpleWorkWithSyncContinue<T>(Func<T> delegatetask, Action<T> continueWith, bool setWorking,
            [CallerMemberName] string taskName = AnonymousTask)
        {
            if (delegatetask == null) throw new ArgumentNullException(nameof(delegatetask));
            return SimpleWorkInternal(new Task<T>(delegatetask.Invoke), new Action<Task<T>>(s =>
                ThreadSaveAction(() => continueWith(s.Result))), taskName, setWorking);
        }

        public Task SimpleWorkWithSyncContinue<T>(Func<T> delegatetask, Action<T> continueWith,
            [CallerMemberName] string taskName = AnonymousTask)
        {
            if (delegatetask == null) throw new ArgumentNullException(nameof(delegatetask));
            return SimpleWorkInternal(new Task<T>(delegatetask.Invoke), new Action<Task<T>>(s =>
                ThreadSaveAction(() => continueWith(s.Result))), taskName, true);
        }

        public Task SimpleWorkWithSyncContinue(Action delegatetask, Action continueWith,
          [CallerMemberName] string taskName = AnonymousTask)
        {
            if (delegatetask == null) throw new ArgumentNullException(nameof(delegatetask));
            return SimpleWorkInternal(new Task(delegatetask.Invoke), new Action<Task>(s =>
                ThreadSaveAction(continueWith)), taskName, true);
        }

        public Task BackgroundSimpleWork(Action delegatetask, [CallerMemberName] string taskName = AnonymousTask)
        {
            if (delegatetask == null) throw new ArgumentNullException(nameof(delegatetask));
            return SimpleWorkInternal(new Task(delegatetask.Invoke), null, taskName, false);
        }

        public Task BackgroundSimpleWork<T>(Func<T> delegatetask, Action<T> continueWith,
            [CallerMemberName] string taskName = AnonymousTask)
        {
            if (delegatetask == null) throw new ArgumentNullException(nameof(delegatetask));
            return SimpleWorkInternal(new Task<T>(delegatetask.Invoke), continueWith, taskName, false);
        }

        public Task SimpleWork(Action delegatetask, [CallerMemberName] string taskName = AnonymousTask)
        {
            if (delegatetask == null) throw new ArgumentNullException(nameof(delegatetask));
            return SimpleWorkInternal(new Task(delegatetask.Invoke), null, taskName, true);
        }

        public Task SimpleWork(Action delegatetask, Action continueWith, [CallerMemberName] string taskName = AnonymousTask)
        {
            if (delegatetask == null) throw new ArgumentNullException(nameof(delegatetask));
            return SimpleWorkInternal(new Task(delegatetask.Invoke), continueWith, taskName, true);
        }

        public Task SimpleWork<T>(Func<T> delegatetask, Action<T> continueWith,
            [CallerMemberName] string taskName = AnonymousTask)
        {
            if (delegatetask == null) throw new ArgumentNullException(nameof(delegatetask));
            return SimpleWorkInternal(new Task<T>(delegatetask.Invoke), new Action<Task<T>>(s => continueWith(s.Result)),
                taskName, true);
        }

        public Task SimpleWork(Delegate delegatetask, Delegate continueWith,
            [CallerMemberName] string taskName = AnonymousTask)
        {
            if (delegatetask == null) throw new ArgumentNullException(nameof(delegatetask));
            return SimpleWorkInternal(new Task(() => delegatetask.DynamicInvoke()), continueWith);
        }

        public Task SimpleWork(Delegate delegatetask, [CallerMemberName] string taskName = AnonymousTask)
        {
            if (delegatetask == null) throw new ArgumentNullException(nameof(delegatetask));
            return SimpleWorkInternal(new Task(() => delegatetask.DynamicInvoke()), null, taskName, true);
        }

        public Task SimpleWork(Task task, Delegate continueWith, [CallerMemberName] string taskName = AnonymousTask,
            bool setWorking = true)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            return SimpleWorkInternal(task, continueWith, taskName, setWorking);
        }

        private Task SimpleWorkInternal(Task task, Delegate continueWith, string taskName = AnonymousTask,
            bool setWorking = true)
        {
            if (task != null)
            {
                lock (_namedTasks)
                {
                    _namedTasks.Add(new Tuple<string, Task>(taskName, task));
                }
                ThreadSaveAction(() =>
                {
                    SendPropertyChanged(() => IsWorkingTask);
                    CommandManager.InvalidateRequerySuggested();
                });
                if (setWorking)
                    StartWork();
                task.ContinueWith(s => CreateContinue(s, continueWith, taskName, setWorking)());
                if (setWorking)
                    CurrentTask = task;
                BackgroundSimpleWork(task);
                return task;
            }
            return null;
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
                    lock (_namedTasks)
                    {
                        var fod = _namedTasks.FirstOrDefault(e => e.Item1 == taskName);
                        _namedTasks.Remove(fod);
                    }

                    if (setWOrking)
                        EndWork();
                    ThreadSaveAction(() =>
                    {
                        SendPropertyChanged(() => IsWorkingTask);
                        CommandManager.InvalidateRequerySuggested();
                    });
                }
            };

            return contuine;
        }

        public void BackgroundSimpleWork(Task task)
        {
            if (task != null)
                task.Start();
        }

        public Task SimpleWork(Task task, [CallerMemberName] string taskName = AnonymousTask)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            return SimpleWorkInternal(task, null, taskName, true);
        }

        public Task SimpleWork(Task task, bool setWorking, [CallerMemberName] string taskName = AnonymousTask)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            return SimpleWorkInternal(task, null, taskName, setWorking);
        }

        #region IsWorking property

        private volatile bool _isWorking = default(bool);

        public bool IsWorking
        {
            get { return _isWorking; }
            set
            {
                _isWorking = value;
                ThreadSaveAction(() =>
                {
                    SendPropertyChanged(() => IsWorking);
                    SendPropertyChanged(() => IsNotWorking);
                    CommandManager.InvalidateRequerySuggested();
                });
            }
        }

        public bool IsWorkingTask
        {
            get { return _namedTasks.Any(); }
        }

        #endregion
    }
}