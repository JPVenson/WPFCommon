#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

#endregion

namespace JPB.WPFBase.MVVM.ViewModel
{
	/// <summary>
	///     Contains Methods for executing background tasks.
	///     All Background Tasks will be executed with the Framework interal Dispatcher Lock.
	///     All Ui Related operations from that tasks are Dispatcher-Thread save if you use the WPFToolsAwesome
	/// </summary>
	public abstract class AsyncViewModelBase : ViewModelBase
	{
		private const string AnonymousTask = "AnonymousTask";

		private List<Tuple<string, Task>> _namedTasks;

		/// <summary>
		///		Creates a new <c>AsyncViewModelBase</c> with the given Dispatcher
		/// </summary>
		/// <param name="disp"></param>
		protected AsyncViewModelBase(Dispatcher disp)
				: base(disp)
		{
			Dispatcher.ShutdownStarted += DispShutdownStarted;
			Init();
		}
		/// <summary>
		///		Creates a new <c>AsyncViewModelBase</c> with ether the Captured dispatcher from the current DispatcherLock or the current Application Dispatcher
		/// </summary>
		protected AsyncViewModelBase() : this(DispatcherLock.GetDispatcher())
		{
		}

		/// <summary>
		///		A Collection of all currently running Tasks
		/// </summary>
		protected IReadOnlyCollection<Tuple<string, Task>> TaskList
		{
			get
			{
				lock (Lock)
				{
					return _namedTasks.AsReadOnly();
				}
			}
		}

		#region IsNotWorking property

		/// <summary>
		///     The negated WorkingFlag
		///     Will be triggerd with SendPropertyChanged/ing
		/// </summary>
		public bool IsNotWorking
		{
			get { return !IsWorking; }
		}

		#endregion

		/// <summary>
		///     Task options for this Instance
		/// </summary>
		protected virtual AsyncViewModelBaseOptions AsyncViewModelBaseOptions { get; set; }

		/// <summary>
		///     Checks the current Task list for the <paramref name="index" />
		/// </summary>
		/// <param name="index">The named task to check</param>
		/// <returns>
		///     <value>True</value>
		///     when a task with the name of <paramref name="index" /> exists otherwise
		///     <value>False</value>
		/// </returns>
		public bool this[string index]
		{
			get { return TaskList.All(s => s.Item1 != index); }
		}

		protected void Init()
		{
			_namedTasks = new List<Tuple<string, Task>>();
			AsyncViewModelBaseOptions = AsyncViewModelBaseOptions.Default();
		}

		internal virtual void DispShutdownStarted(object sender, EventArgs e)
		{
			try
			{
				foreach (var namedTask in TaskList)
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

		/// <summary>
		///     Allows you to check for a Condtion if the calling method is named after the mehtod you would like to check but
		///     starts with "Can"
		/// </summary>
		/// <param name="taskName"></param>
		/// <returns></returns>
		public bool CheckCanExecuteCondition([CallerMemberName] string taskName = AnonymousTask)
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

		/// <summary>
		///     Will be executed right before a Task is started
		/// </summary>
		protected virtual void StartWork()
		{
			IsWorking = true;
		}

		/// <summary>
		///     Will be executed right before a Task is finshed
		/// </summary>
		protected virtual void EndWork()
		{
			IsWorking = TaskList.Any();
		}

		/// <summary>
		///     Override this to handle exceptions thrown by any Task worker function
		/// </summary>
		/// <param name="exception"></param>
		/// <returns>
		///     <value>True</value>
		///     if the exception was Handled otherwise
		///     <value>False</value>
		///     . If false the exception will be bubbled to the caller
		/// </returns>
		protected virtual bool OnTaskException(Exception exception)
		{
			return false;
		}

		/// <summary>
		///     Runs the <paramref name="delegatetask" /> and schedules the <paramref name="continueWith" /> in the Dispatcher
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
			if (delegatetask == null)
			{
				throw new ArgumentNullException(nameof(delegatetask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return SimpleWorkInternal(delegatetask, s =>
					ThreadSaveAction(() => continueWith(s.Result)), taskName, setWorking);
		}

		/// <summary>
		///     Runs the <paramref name="delegatetask" /> and schedules the <paramref name="continueWith" /> in the Dispatcher
		/// </summary>
		/// <param name="delegatetask"></param>
		/// <param name="continueWith"></param>
		/// <param name="setWorking"></param>
		/// <param name="taskName"></param>
		/// <returns>The created and running Task</returns>
		public Task SimpleWorkWithSyncContinue(Task delegatetask, Action continueWith, bool setWorking,
				[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegatetask == null)
			{
				throw new ArgumentNullException(nameof(delegatetask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return SimpleWorkInternal(delegatetask, s =>
					ThreadSaveAction(continueWith), taskName, setWorking);
		}

		/// <summary>
		///     Runs the <paramref name="delegatetask" /> in a task and schedules the <paramref name="continueWith" /> in the
		///     Dispatcher
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
			if (delegatetask == null)
			{
				throw new ArgumentNullException(nameof(delegatetask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return SimpleWorkInternal(CreateNewTask(delegatetask), s =>
					ThreadSaveAction(() => continueWith(s.Result)), taskName, setWorking);
		}

		private Task<T> CreateNewTask<T>(Func<T> delegatetask)
		{
			var hasLock = DispatcherLock.Current;
			return AsyncViewModelBaseOptions.TaskFactory.StartNew(() =>
			{
				if (hasLock != null)
				{
					DispatcherLock.Current = hasLock;
				}

				return delegatetask.Invoke();
			});
		}

		private Task CreateNewTask(Action delegatetask)
		{
			var hasLock = DispatcherLock.Current;
			return AsyncViewModelBaseOptions.TaskFactory.StartNew(() =>
			{
				if (hasLock != null)
				{
					DispatcherLock.Current = hasLock;
				}

				delegatetask.Invoke();
			});
		}

		/// <summary>
		///     Runs the <paramref name="delegatetask" /> in a task and schedules the <paramref name="continueWith" /> in the
		///     Dispatcher
		/// </summary>
		/// <param name="delegatetask"></param>
		/// <param name="continueWith"></param>
		/// <param name="setWorking"></param>
		/// <param name="taskName"></param>
		/// <returns>The created and running Task</returns>
		public Task SimpleWorkWithSyncContinueAsync(Func<Task> delegatetask, Action continueWith, bool setWorking,
				[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegatetask == null)
			{
				throw new ArgumentNullException(nameof(delegatetask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return
					SimpleWorkInternal(CreateNewTask(() => delegatetask.Invoke().Wait()), s =>
							ThreadSaveAction(continueWith), taskName, setWorking);
		}

		/// <summary>
		///     Runs the <paramref name="delegatetask" /> in a task and schedules the <paramref name="continueWith" /> in the
		///     Dispatcher
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="delegatetask"></param>
		/// <param name="continueWith"></param>
		/// <param name="setWorking"></param>
		/// <param name="taskName"></param>
		/// <returns>The created and running Task</returns>
		public Task SimpleWorkWithSyncContinueAsync<T>(Func<Task<T>> delegatetask, Action<T> continueWith,
				bool setWorking,
				[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegatetask == null)
			{
				throw new ArgumentNullException(nameof(delegatetask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return
					SimpleWorkInternal(CreateNewTask(delegatetask.Invoke),
					t => continueWith(t.Result.Result), taskName, setWorking);
		}

		/// <summary>
		///     Runs the <paramref name="delegatetask" /> in a task and schedules the <paramref name="continueWith" /> in the
		///     Dispatcher
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="delegatetask"></param>
		/// <param name="continueWith"></param>
		/// <param name="taskName"></param>
		/// <returns>The created and running Task</returns>
		public Task SimpleWorkWithSyncContinue<T>(Func<T> delegatetask, Action<T> continueWith,
				[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegatetask == null)
			{
				throw new ArgumentNullException(nameof(delegatetask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return SimpleWorkInternal(CreateNewTask(delegatetask.Invoke), s =>
					ThreadSaveAction(() => continueWith(s.Result)), taskName);
		}

		/// <summary>
		///     Runs the <paramref name="delegatetask" /> in a task and schedules the <paramref name="continueWith" /> in the
		///     Dispatcher
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="delegatetask"></param>
		/// <param name="continueWith"></param>
		/// <param name="taskName"></param>
		/// <returns>The created and running Task</returns>
		public Task SimpleWorkWithSyncContinueAsync<T>(Func<Task<T>> delegatetask, Action<T> continueWith,
				[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegatetask == null)
			{
				throw new ArgumentNullException(nameof(delegatetask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return
					SimpleWorkInternal(CreateNewTask(delegatetask.Invoke),
					t => continueWith(t.Result.Result), taskName);
		}

		/// <summary>
		///     Runs the <paramref name="delegatetask" /> in a task and schedules the <paramref name="continueWith" /> in the
		///     Dispatcher
		/// </summary>
		/// <param name="delegatetask"></param>
		/// <param name="continueWith"></param>
		/// <param name="taskName"></param>
		/// <returns>The created and running Task</returns>
		public Task SimpleWorkWithSyncContinue(Action delegatetask, Action continueWith,
				[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegatetask == null)
			{
				throw new ArgumentNullException(nameof(delegatetask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return SimpleWorkInternal(CreateNewTask(delegatetask.Invoke), s =>
					ThreadSaveAction(continueWith), taskName);
		}

		/// <summary>
		///     Runs the <paramref name="delegatetask" /> in a task. Does not set the IsWorking Flag
		/// </summary>
		/// <param name="delegatetask"></param>
		/// <param name="taskName"></param>
		/// <returns></returns>
		public Task BackgroundSimpleWork(Action delegatetask, [CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegatetask == null)
			{
				throw new ArgumentNullException(nameof(delegatetask));
			}

			return SimpleWorkInternal(CreateNewTask(delegatetask.Invoke), null,
			taskName, false);
		}

		/// <summary>
		///     Runs the <paramref name="delegatetask" /> in a task. Does not set the IsWorking Flag
		/// </summary>
		/// <param name="delegatetask"></param>
		/// <param name="continueWith"></param>
		/// <param name="taskName"></param>
		/// <returns></returns>
		public Task BackgroundSimpleWork<T>(Func<T> delegatetask, Action<T> continueWith,
				[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegatetask == null)
			{
				throw new ArgumentNullException(nameof(delegatetask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return SimpleWorkInternal(CreateNewTask(delegatetask.Invoke),
			t => continueWith(t.Result), taskName, false);
		}

		/// <summary>
		///     Runs the <paramref name="delegatetask" /> in a task. Does not set the IsWorking Flag
		/// </summary>
		/// <param name="delegatetask"></param>
		/// <param name="continueWith"></param>
		/// <param name="taskName"></param>
		/// <returns></returns>
		public Task BackgroundSimpleWorkAsync<T>(Func<Task<T>> delegatetask, Action<T> continueWith,
				[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegatetask == null)
			{
				throw new ArgumentNullException(nameof(delegatetask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return
					SimpleWorkInternal(CreateNewTask(delegatetask.Invoke),
					t => continueWith(t.Result.Result), taskName, false);
		}

		/// <summary>
		///     Runs the <paramref name="delegatetask" />
		/// </summary>
		/// <param name="delegatetask"></param>
		/// <param name="taskName"></param>
		/// <returns></returns>
		public Task SimpleWork(Action delegatetask, [CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegatetask == null)
			{
				throw new ArgumentNullException(nameof(delegatetask));
			}

			return SimpleWorkInternal(CreateNewTask(delegatetask.Invoke), null,
			taskName);
		}

		/// <summary>
		///     Runs the <paramref name="delegatetask" /> and executes the continueWith after that
		/// </summary>
		/// <param name="delegatetask"></param>
		/// <param name="continueWith"></param>
		/// <param name="taskName"></param>
		/// <returns></returns>
		public Task SimpleWork(Action delegatetask, Action continueWith,
				[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegatetask == null)
			{
				throw new ArgumentNullException(nameof(delegatetask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return SimpleWorkInternal(CreateNewTask(delegatetask.Invoke),
			task => continueWith(), taskName);
		}

		/// <summary>
		///     Runs the <paramref name="delegatetask" /> and executes the continueWith after that
		/// </summary>
		/// <param name="delegatetask"></param>
		/// <param name="continueWith"></param>
		/// <param name="taskName"></param>
		/// <returns></returns>
		public Task SimpleWork<T>(Func<T> delegatetask, Action<T> continueWith,
				[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegatetask == null)
			{
				throw new ArgumentNullException(nameof(delegatetask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return SimpleWorkInternal(CreateNewTask(delegatetask.Invoke),
			s => continueWith(s.Result),
			taskName);
		}

		/// <summary>
		///     Runs the <paramref name="delegatetask" /> and executes the continueWith after that
		/// </summary>
		/// <param name="delegatetask"></param>
		/// <param name="continueWith"></param>
		/// <param name="taskName"></param>
		/// <returns></returns>
		public Task SimpleWorkAsync(Func<Task> delegatetask, Action continueWith,
				[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegatetask == null)
			{
				throw new ArgumentNullException(nameof(delegatetask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return
					SimpleWorkInternal(CreateNewTask(() => delegatetask.Invoke().Wait()),
					task => continueWith(),
					taskName);
		}

		/// <summary>
		///     Runs the <paramref name="delegatetask" /> and executes the continueWith after that
		/// </summary>
		/// <param name="delegatetask"></param>
		/// <param name="taskName"></param>
		/// <returns></returns>
		public Task SimpleWorkAsync(Func<Task> delegatetask,
				[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegatetask == null)
			{
				throw new ArgumentNullException(nameof(delegatetask));
			}

			return
					SimpleWorkInternal(CreateNewTask(() => delegatetask.Invoke().Wait()),
					null,
					taskName);
		}

		/// <summary>
		///     Runs the <paramref name="delegatetask" /> and executes the continueWith after that
		/// </summary>
		/// <param name="delegatetask"></param>
		/// <param name="continueWith"></param>
		/// <param name="taskName"></param>
		/// <returns></returns>
		public Task SimpleWorkAsync<T>(Func<Task<T>> delegatetask, Action<T> continueWith,
				[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegatetask == null)
			{
				throw new ArgumentNullException(nameof(delegatetask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return
					SimpleWorkInternal(CreateNewTask(delegatetask.Invoke),
					s => continueWith(s.Result.Result),
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
			if (delegatetask == null)
			{
				throw new ArgumentNullException(nameof(delegatetask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return SimpleWorkInternal(CreateNewTask(async () =>
			{
				var dynamicInvokeResult = delegatetask.DynamicInvoke();
				if (dynamicInvokeResult is Task)
				{
					await (dynamicInvokeResult as Task);
				}
			}), task => continueWith.DynamicInvoke());
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
		///     Runs the <paramref name="task" /> and executes the continueWith after that
		/// </summary>
		/// <param name="task">An Started or Not started task</param>
		/// <param name="continueWith"></param>
		/// <param name="taskName"></param>
		/// <param name="setWorking"></param>
		/// <returns></returns>
		public Task SimpleWork(Task task, Delegate continueWith, [CallerMemberName] string taskName = AnonymousTask,
				bool setWorking = true)
		{
			if (task == null)
			{
				throw new ArgumentNullException(nameof(task));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return SimpleWorkInternal(task, t => continueWith.DynamicInvoke(), taskName, setWorking);
		}

		/// <summary>
		///     Runs the <paramref name="task" /> and executes the continueWith after that
		/// </summary>
		/// <param name="task">An Started or Not started task</param>
		/// <param name="continueWith"></param>
		/// <param name="taskName"></param>
		/// <param name="setWorking"></param>
		/// <returns></returns>
		public Task SimpleWork(Task task, Action continueWith, [CallerMemberName] string taskName = AnonymousTask,
				bool setWorking = true)
		{
			if (task == null)
			{
				throw new ArgumentNullException(nameof(task));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return SimpleWorkInternal(task, t => continueWith(), taskName, setWorking);
		}

		private Task SimpleWorkInternal<T>(Task<T> task, Action<Task<T>> continueWith, string taskName = AnonymousTask,
				bool setWorking = true)
		{
			return SimpleWorkInternal(task as Task, t => continueWith(task), taskName, setWorking);
		}

		public AwaitMultibe BeginScope()
		{
			return new AwaitMultibe(this);
		}

		public event EventHandler<Task> TaskCreated;
		public event EventHandler<Task> TaskDone;

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
				{
					StartWork();
				}

				task.ContinueWith(s => CreateContinue(s, continueWith, taskName, setWorking)());
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
						{
							return;
						}

						s.Exception?.Handle(OnTaskException);
					}

					continueWith?.Invoke(s);
				}
				finally
				{
					lock (_namedTasks)
					{
						var fod = _namedTasks.FirstOrDefault(e => e.Item1 == taskName);
						_namedTasks.Remove(fod);
					}

					if (setWOrking)
					{
						EndWork();
					}

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
			OnTaskCreated(task);
			if (task != null && task.Status == TaskStatus.Created)
			{
				task.Start();
			}
		}

		/// <summary>
		///     Runs the <paramref name="task" />
		/// </summary>
		/// <param name="task">An Started or Not started task</param>
		/// <param name="taskName"></param>
		/// <returns></returns>
		public Task SimpleWork(Task task, [CallerMemberName] string taskName = AnonymousTask)
		{
			if (task == null)
			{
				throw new ArgumentNullException(nameof(task));
			}

			return SimpleWorkInternal(task, null, taskName);
		}

		/// <summary>
		///     Runs the <paramref name="task" />
		/// </summary>
		/// <param name="task">An Started or Not started task</param>
		/// <param name="setWorking"></param>
		/// <param name="taskName"></param>
		/// <returns></returns>
		public Task SimpleWork(Task task, bool setWorking, [CallerMemberName] string taskName = AnonymousTask)
		{
			if (task == null)
			{
				throw new ArgumentNullException(nameof(task));
			}

			return SimpleWorkInternal(task, null, taskName, setWorking);
		}

		protected virtual void OnTaskCreated(Task e)
		{
			TaskCreated?.Invoke(this, e);
		}

		protected virtual void OnTaskDone(Task e)
		{
			TaskDone?.Invoke(this, e);
		}

		public class AwaitMultibe : IEnumerable<Task>, IDisposable
		{
			private readonly AsyncViewModelBase _sender;

			private readonly List<Task> _tasks;

			public AwaitMultibe(AsyncViewModelBase sender)
			{
				_sender = sender;
				_tasks = new List<Task>();
				_sender.TaskCreated += _sender_TaskCreated;
			}

			public void Dispose()
			{
				_sender.TaskCreated -= _sender_TaskCreated;
			}

			public IEnumerator<Task> GetEnumerator()
			{
				return _tasks.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable) _tasks).GetEnumerator();
			}

			private void _sender_TaskCreated(object sender, Task e)
			{
				_tasks.Add(e);
			}

			public async Task GetAwaiter()
			{
				await Task.WhenAll(_tasks);
			}
		}

		#region IsWorking property

		private volatile bool _isWorking = default(bool);

		/// <summary>
		///     Indicates that this Manager is currently working or not
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

		public bool IsWorkingTask => TaskList.Any();

		#endregion
	}
}