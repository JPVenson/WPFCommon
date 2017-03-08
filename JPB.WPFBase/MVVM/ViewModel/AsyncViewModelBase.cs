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
    public class AsyncViewModelBaseOptions
    {
        static AsyncViewModelBaseOptions()
        {
            DefaultOptions = Default();
        }

        public AsyncViewModelBaseOptions(TaskScheduler taskScheduler, TaskFactory taskFactory)
        {
            TaskScheduler = taskScheduler;
            TaskFactory = taskFactory;
        }

        public TaskScheduler TaskScheduler { get; private set; }
        public TaskFactory TaskFactory { get; private set; }

        public static AsyncViewModelBaseOptions DefaultOptions { get; set; }

        public static AsyncViewModelBaseOptions Default()
        {
            var scheduler = TaskScheduler.Default;
            var factory = new TaskFactory(scheduler);
            return new AsyncViewModelBaseOptions(scheduler, factory);
        }
    }

    public abstract class AsyncViewModelBase : ViewModelBase
    {
        private const string AnonymousTask = "AnonymousTask";

        private List<Tuple<string, Task>> _namedTasks;

        protected volatile Task CurrentTask;

        protected AsyncViewModelBase(Dispatcher disp)
            : base(disp)
        {
            disp.ShutdownFinished += disp_ShutdownStarted;
            Init();
        }

        protected AsyncViewModelBase()
        {
            Init();
        }

        protected void Init()
        {
            _namedTasks = new List<Tuple<string, Task>>();
            AsyncViewModelBaseOptions = AsyncViewModelBaseOptions.Default();
        }

        #region IsNotWorking property

        /// <summary>
        /// The negated WorkingFlag
        /// Will be triggerd with SendPropertyChanged/ing
        /// </summary>
        public bool IsNotWorking
        {
            get { return !IsWorking; }
        }

        #endregion

        /// <summary>
        /// Task options for this Instance
        /// </summary>
        protected virtual AsyncViewModelBaseOptions AsyncViewModelBaseOptions { get;  set; }

        /// <summary>
        /// Checks the current Task list for the <paramref name="index"/>
        /// </summary>
        /// <param name="index">The named task to check</param>
        /// <returns><value>True</value> when a task with the name of <paramref name="index"/> exists otherwise <value>False</value></returns>
        public bool this[string index]
        {
            get { return _namedTasks.All(s => s.Item1 != index); }
        }

        private void disp_ShutdownStarted(object sender, EventArgs e)
        {
            try
            {
                foreach (var namedTask in _namedTasks)
                    if (namedTask.Item2.Status == TaskStatus.Running)
                    {
#if !WINDOWS_UWP
                        namedTask.Item2.Dispose();
#endif
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


        /// <summary>
        /// Will be executed right before a Task is started
        /// </summary>
        protected virtual void StartWork()
        {
            IsWorking = true;
        }

        /// <summary>
        /// Will be executed right before a Task is finshed
        /// </summary>
        protected virtual void EndWork()
        {
            IsWorking = _namedTasks.Any();
        }

        /// <summary>
        /// Override this to handle exceptions thrown by any Task worker function
        /// </summary>
        /// <param name="exception"></param>
        /// <returns><value>True</value> if the exception was Handled otherwise <value>False</value>. If false the exception will be bubbled to the caller</returns>
        protected virtual bool OnTaskException(Exception exception)
        {
            return false;
        }

        /// <summary>
        /// Runs the <paramref name="delegatetask"/> and schedules the <paramref name="continueWith"/> in the Dispatcher
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="delegatetask"></param>
        /// <param name="continueWith"></param>
        /// <param name="setWorking"></param>
        /// <param name="taskName"></param>
        /// <returns>The created and running Task</returns>
        public Task SimpleWorkWithSyncContinue<T>(Task<T> delegatetask, Action<T> continueWith, bool setWorking,
            [CallerMemberName] string taskName = AnonymousTask)
        {
            if (delegatetask == null) throw new ArgumentNullException(nameof(delegatetask));
            if (continueWith == null) throw new ArgumentNullException(nameof(continueWith));
            return SimpleWorkInternal(delegatetask, s =>
                ThreadSaveAction(() => continueWith(s.Result)), taskName, setWorking);
        }

        /// <summary>
        /// Runs the <paramref name="delegatetask"/> and schedules the <paramref name="continueWith"/> in the Dispatcher
        /// </summary>
        /// <param name="delegatetask"></param>
        /// <param name="continueWith"></param>
        /// <param name="setWorking"></param>
        /// <param name="taskName"></param>
        /// <returns>The created and running Task</returns>
        public Task SimpleWorkWithSyncContinue(Task delegatetask, Action continueWith, bool setWorking,
            [CallerMemberName] string taskName = AnonymousTask)
        {
            if (delegatetask == null) throw new ArgumentNullException(nameof(delegatetask));
            if (continueWith == null) throw new ArgumentNullException(nameof(continueWith));
            return SimpleWorkInternal(delegatetask, s =>
                ThreadSaveAction(continueWith), taskName, setWorking);
        }

        /// <summary>
        /// Runs the <paramref name="delegatetask"/> in a task and schedules the <paramref name="continueWith"/> in the Dispatcher
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="delegatetask"></param>
        /// <param name="continueWith"></param>
        /// <param name="setWorking"></param>
        /// <param name="taskName"></param>
        /// <returns>The created and running Task</returns>
        public Task SimpleWorkWithSyncContinue<T>(Func<T> delegatetask, Action<T> continueWith, bool setWorking,
            [CallerMemberName] string taskName = AnonymousTask)
        {
            if (delegatetask == null) throw new ArgumentNullException(nameof(delegatetask));
            if (continueWith == null) throw new ArgumentNullException(nameof(continueWith));
            return SimpleWorkInternal(AsyncViewModelBaseOptions.TaskFactory.StartNew(delegatetask.Invoke), s =>
                ThreadSaveAction(() => continueWith(s.Result)), taskName, setWorking);
        }

        /// <summary>
        /// Runs the <paramref name="delegatetask"/> in a task and schedules the <paramref name="continueWith"/> in the Dispatcher
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="delegatetask"></param>
        /// <param name="continueWith"></param>
        /// <param name="taskName"></param>
        /// <returns>The created and running Task</returns>
        public Task SimpleWorkWithSyncContinue<T>(Func<T> delegatetask, Action<T> continueWith,
            [CallerMemberName] string taskName = AnonymousTask)
        {
            if (delegatetask == null) throw new ArgumentNullException(nameof(delegatetask));
            if (continueWith == null) throw new ArgumentNullException(nameof(continueWith));
            return SimpleWorkInternal(AsyncViewModelBaseOptions.TaskFactory.StartNew(delegatetask.Invoke), s =>
                ThreadSaveAction(() => continueWith(s.Result)), taskName);
        }

        /// <summary>
        /// Runs the <paramref name="delegatetask"/> in a task and schedules the <paramref name="continueWith"/> in the Dispatcher
        /// </summary>
        /// <param name="delegatetask"></param>
        /// <param name="continueWith"></param>
        /// <param name="taskName"></param>
        /// <returns>The created and running Task</returns>
        public Task SimpleWorkWithSyncContinue(Action delegatetask, Action continueWith,
            [CallerMemberName] string taskName = AnonymousTask)
        {
            if (delegatetask == null) throw new ArgumentNullException(nameof(delegatetask));
            if (continueWith == null) throw new ArgumentNullException(nameof(continueWith));
            return SimpleWorkInternal(AsyncViewModelBaseOptions.TaskFactory.StartNew(delegatetask.Invoke), s =>
                ThreadSaveAction(continueWith), taskName);
        }
        /// <summary>
        /// Runs the <paramref name="delegatetask"/> in a task. Does not set the IsWorking Flag
        /// </summary>
        /// <param name="delegatetask"></param>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public Task BackgroundSimpleWork(Action delegatetask, [CallerMemberName] string taskName = AnonymousTask)
        {
            if (delegatetask == null) throw new ArgumentNullException(nameof(delegatetask));
            return SimpleWorkInternal(AsyncViewModelBaseOptions.TaskFactory.StartNew(delegatetask.Invoke), null, taskName, false);
        }

        /// <summary>
        /// Runs the <paramref name="delegatetask"/> in a task. Does not set the IsWorking Flag
        /// </summary>
        /// <param name="delegatetask"></param>
        /// <param name="continueWith"></param>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public Task BackgroundSimpleWork<T>(Func<T> delegatetask, Action<T> continueWith,
            [CallerMemberName] string taskName = AnonymousTask)
        {
            if (delegatetask == null) throw new ArgumentNullException(nameof(delegatetask));
            if (continueWith == null) throw new ArgumentNullException(nameof(continueWith));
            return SimpleWorkInternal(AsyncViewModelBaseOptions.TaskFactory.StartNew(delegatetask.Invoke), t => continueWith(t.Result), taskName, false);
        }

        /// <summary>
        /// Runs the <paramref name="delegatetask"/>
        /// </summary>
        /// <param name="delegatetask"></param>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public Task SimpleWork(Action delegatetask, [CallerMemberName] string taskName = AnonymousTask)
        {
            if (delegatetask == null) throw new ArgumentNullException(nameof(delegatetask));
            return SimpleWorkInternal(AsyncViewModelBaseOptions.TaskFactory.StartNew(delegatetask.Invoke), null, taskName);
        }

        /// <summary>
        /// Runs the <paramref name="delegatetask"/> and executes the continueWith after that
        /// </summary>
        /// <param name="delegatetask"></param>
        /// <param name="continueWith"></param>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public Task SimpleWork(Action delegatetask, Action continueWith,
            [CallerMemberName] string taskName = AnonymousTask)
        {
            if (delegatetask == null) throw new ArgumentNullException(nameof(delegatetask));
            if (continueWith == null) throw new ArgumentNullException(nameof(continueWith));
            return SimpleWorkInternal(AsyncViewModelBaseOptions.TaskFactory.StartNew(delegatetask.Invoke), task => continueWith(), taskName);
        }
        /// <summary>
        /// Runs the <paramref name="delegatetask"/> and executes the continueWith after that
        /// </summary>
        /// <param name="delegatetask"></param>
        /// <param name="continueWith"></param>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public Task SimpleWork<T>(Func<T> delegatetask, Action<T> continueWith,
            [CallerMemberName] string taskName = AnonymousTask)
        {
            if (delegatetask == null) throw new ArgumentNullException(nameof(delegatetask));
            if (continueWith == null) throw new ArgumentNullException(nameof(continueWith));
            return SimpleWorkInternal(AsyncViewModelBaseOptions.TaskFactory.StartNew(delegatetask.Invoke), s => continueWith(s.Result),
                taskName);
        }

        /// <summary>
        ///     Creates a Background Task
        /// </summary>
        /// <param name="delegatetask">The Delegate that should be executed async.</param>
        /// <param name="continueWith">
        ///     The Delegate that should be executed when <paramref name="delegatetask" /> is done. Must
        ///     accept an Task as first argument
        /// </param>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public Task SimpleWork(Delegate delegatetask, Delegate continueWith,
            [CallerMemberName] string taskName = AnonymousTask)
        {
            if (delegatetask == null) throw new ArgumentNullException(nameof(delegatetask));
            if (continueWith == null) throw new ArgumentNullException(nameof(continueWith));
            return SimpleWorkInternal(AsyncViewModelBaseOptions.TaskFactory.StartNew(() => delegatetask.DynamicInvoke()), task => continueWith.DynamicInvoke());
        }
        ///// <summary>
        ///// Runs the <paramref name="delegatetask"/> and executes the continueWith after that
        ///// </summary>
        ///// <param name="delegatetask">A Delegate that should not accept an Argument</param>
        ///// <param name="taskName"></param>
        ///// <returns></returns>
        //public Task SimpleWork(Delegate delegatetask, [CallerMemberName] string taskName = AnonymousTask)
        //{
        //    if (delegatetask == null) throw new ArgumentNullException(nameof(delegatetask));
        //    return SimpleWorkInternal(AsyncViewModelBaseOptions.TaskFactory.StartNew(() => delegatetask.DynamicInvoke()), null, taskName, true);
        //}
        /// <summary>
        /// Runs the <paramref name="task"/> and executes the continueWith after that
        /// </summary>
        /// <param name="task">An Started or Not started task</param>
        /// <param name="continueWith"></param>
        /// <param name="taskName"></param>
        /// <param name="setWorking"></param>
        /// <returns></returns>
        public Task SimpleWork(Task task, Delegate continueWith, [CallerMemberName] string taskName = AnonymousTask,
            bool setWorking = true)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (continueWith == null) throw new ArgumentNullException(nameof(continueWith));
            return SimpleWorkInternal(task, t => continueWith.DynamicInvoke(), taskName, setWorking);
        }

        /// <summary>
        /// Runs the <paramref name="task"/> and executes the continueWith after that
        /// </summary>
        /// <param name="task">An Started or Not started task</param>
        /// <param name="continueWith"></param>
        /// <param name="taskName"></param>
        /// <param name="setWorking"></param>
        /// <returns></returns>
        public Task SimpleWork(Task task, Action continueWith, [CallerMemberName] string taskName = AnonymousTask,
            bool setWorking = true)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (continueWith == null) throw new ArgumentNullException(nameof(continueWith));
            return SimpleWorkInternal(task, t => continueWith(), taskName, setWorking);
        }

        private Task SimpleWorkInternal<T>(Task<T> task, Action<Task<T>> continueWith, string taskName = AnonymousTask,
            bool setWorking = true)
        {
            return SimpleWorkInternal(task as Task, t => continueWith(task), taskName, setWorking);
        }

        private Task SimpleWorkInternal(Task task, Action<Task> continueWith, string taskName = AnonymousTask,
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

        private Action CreateContinue(Task s, Action<Task> continueWith, string taskName, bool setWOrking = true)
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
                        continueWith.Invoke(s);
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

        private void BackgroundSimpleWork(Task task)
        {
            if (task != null && task.Status == TaskStatus.Created)
                task.Start();
        }
        /// <summary>
        /// Runs the <paramref name="task"/>
        /// </summary>
        /// <param name="task">An Started or Not started task</param>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public Task SimpleWork(Task task, [CallerMemberName] string taskName = AnonymousTask)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            return SimpleWorkInternal(task, null, taskName);
        }

        /// <summary>
        /// Runs the <paramref name="task"/>
        /// </summary>
        /// <param name="task">An Started or Not started task</param>
        /// <param name="setWorking"></param>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public Task SimpleWork(Task task, bool setWorking, [CallerMemberName] string taskName = AnonymousTask)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            return SimpleWorkInternal(task, null, taskName, setWorking);
        }

        #region IsWorking property

        private volatile bool _isWorking = default(bool);

        /// <summary>
        /// Indicates that this Manager is currently working or not
        /// </summary>
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