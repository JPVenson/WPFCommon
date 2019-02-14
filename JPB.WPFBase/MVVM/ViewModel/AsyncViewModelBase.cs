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
using JetBrains.Annotations;

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
		///     Creates a new <c>AsyncViewModelBase</c> with the given Dispatcher
		/// </summary>
		/// <param name="dispatcher"></param>
		protected AsyncViewModelBase(Dispatcher dispatcher)
			: base(dispatcher)
		{
			Dispatcher.ShutdownStarted += DispatcherShutdownStarted;
			Init();
		}

		/// <summary>
		///     Creates a new <c>AsyncViewModelBase</c> with ether the Captured dispatcher from the current DispatcherLock or the
		///     current Application Dispatcher
		/// </summary>
		protected AsyncViewModelBase() 
			: this(DispatcherLock.GetDispatcher())
		{
		}

		/// <summary>
		///     A Collection of all currently running Tasks
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
		///     Will be trigger with SendPropertyChanged/ing
		/// </summary>
		public bool IsNotWorking
		{
			get { return !IsWorking; }
		}

		#endregion

		/// <summary>
		///     Task options for this Instance
		/// </summary>
		[NotNull]
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

		/// <summary>
		///     Initializes this instance.
		/// </summary>
		protected void Init()
		{
			_namedTasks = new List<Tuple<string, Task>>();
			AsyncViewModelBaseOptions = AsyncViewModelBaseOptions.DefaultOptions;
		}

		internal virtual void DispatcherShutdownStarted(object sender, EventArgs e)
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
		[UsedImplicitly]
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
		///     Runs the <paramref name="delegateTask" /> and schedules the <paramref name="continueWith" /> in the Dispatcher
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="delegateTask"></param>
		/// <param name="continueWith"></param>
		/// <param name="setWorking"></param>
		/// <param name="taskName"></param>
		/// <returns>The created and running Task</returns>
		[NotNull]
		[PublicAPI]
		public Task SimpleWorkWithSyncContinue<T>([NotNull] Task<T> delegateTask, [NotNull] Action<T> continueWith,
			bool setWorking,
			[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegateTask == null)
			{
				throw new ArgumentNullException(nameof(delegateTask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return SimpleWorkInternal(delegateTask, s =>
				ThreadSaveAction(() => continueWith(s.Result)), taskName, setWorking);
		}

		/// <summary>
		///     Runs the <paramref name="delegateTask" /> and schedules the <paramref name="continueWith" /> in the Dispatcher
		/// </summary>
		/// <param name="delegateTask"></param>
		/// <param name="continueWith"></param>
		/// <param name="setWorking"></param>
		/// <param name="taskName"></param>
		/// <returns>The created and running Task</returns>
		[NotNull]
		[PublicAPI]
		public Task SimpleWorkWithSyncContinue([NotNull] Task delegateTask, [NotNull] Action continueWith,
			bool setWorking,
			[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegateTask == null)
			{
				throw new ArgumentNullException(nameof(delegateTask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return SimpleWorkInternal(delegateTask, s =>
				ThreadSaveAction(continueWith), taskName, setWorking);
		}

		/// <summary>
		///     Runs the <paramref name="delegateTask" /> in a task and schedules the <paramref name="continueWith" /> in the
		///     Dispatcher
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="delegateTask"></param>
		/// <param name="continueWith"></param>
		/// <param name="setWorking"></param>
		/// <param name="taskName"></param>
		/// <returns>The created and running Task</returns>
		[NotNull]
		[PublicAPI]
		public Task SimpleWorkWithSyncContinue<T>([NotNull] Func<T> delegateTask, [NotNull] Action<T> continueWith,
			bool setWorking,
			[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegateTask == null)
			{
				throw new ArgumentNullException(nameof(delegateTask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return SimpleWorkInternal(CreateNewAsyncTask(delegateTask), s =>
				ThreadSaveAction(() => continueWith(s.Result)), taskName, setWorking);
		}

		private Task<T> CreateNewAsyncTask<T>(Func<T> delegateTask)
		{
			var hasLock = DispatcherLock.Current;
			return AsyncViewModelBaseOptions.TaskFactory.StartNew(() =>
			{
				if (hasLock != null)
				{
					DispatcherLock.Current = hasLock;
				}

				return delegateTask.Invoke();
			});
		}

		private Task CreateNewTask(Action delegateTask)
		{
			var hasLock = DispatcherLock.Current;
			return AsyncViewModelBaseOptions.TaskFactory.StartNew(() =>
			{
				if (hasLock != null)
				{
					DispatcherLock.Current = hasLock;
				}

				delegateTask.Invoke();
			});
		}

		/// <summary>
		///     Runs the <paramref name="delegateTask" /> in a task and schedules the <paramref name="continueWith" /> in the
		///     Dispatcher
		/// </summary>
		/// <param name="delegateTask"></param>
		/// <param name="continueWith"></param>
		/// <param name="setWorking"></param>
		/// <param name="taskName"></param>
		/// <returns>The created and running Task</returns>
		[NotNull]
		[PublicAPI]
		public Task SimpleWorkWithSyncContinueAsync([NotNull] Func<Task> delegateTask, [NotNull] Action continueWith,
			bool setWorking,
			[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegateTask == null)
			{
				throw new ArgumentNullException(nameof(delegateTask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return
				SimpleWorkInternal(CreateNewTask(() => delegateTask.Invoke().Wait()), s =>
					ThreadSaveAction(continueWith), taskName, setWorking);
		}

		/// <summary>
		///     Runs the <paramref name="delegateTask" /> in a task and schedules the <paramref name="continueWith" /> in the
		///     Dispatcher
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="delegateTask"></param>
		/// <param name="continueWith"></param>
		/// <param name="setWorking"></param>
		/// <param name="taskName"></param>
		/// <returns>The created and running Task</returns>
		[NotNull]
		[PublicAPI]
		public Task SimpleWorkWithSyncContinueAsync<T>([NotNull] Func<Task<T>> delegateTask,
			[NotNull] Action<T> continueWith,
			bool setWorking,
			[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegateTask == null)
			{
				throw new ArgumentNullException(nameof(delegateTask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return
				SimpleWorkInternal(CreateNewAsyncTask(delegateTask.Invoke),
					t => continueWith(t.Result.Result), taskName, setWorking);
		}

		/// <summary>
		///     Runs the <paramref name="delegateTask" /> in a task and schedules the <paramref name="continueWith" /> in the
		///     Dispatcher
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="delegateTask"></param>
		/// <param name="continueWith"></param>
		/// <param name="taskName"></param>
		/// <returns>The created and running Task</returns>
		[NotNull]
		[PublicAPI]
		public Task SimpleWorkWithSyncContinue<T>([NotNull] Func<T> delegateTask, [NotNull] Action<T> continueWith,
			[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegateTask == null)
			{
				throw new ArgumentNullException(nameof(delegateTask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return SimpleWorkInternal(CreateNewAsyncTask(delegateTask.Invoke), s =>
				ThreadSaveAction(() => continueWith(s.Result)), taskName);
		}

		/// <summary>
		///     Runs the <paramref name="delegateTask" /> in a task and schedules the <paramref name="continueWith" /> in the
		///     Dispatcher
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="delegateTask"></param>
		/// <param name="continueWith"></param>
		/// <param name="taskName"></param>
		/// <returns>The created and running Task</returns>
		[NotNull]
		[PublicAPI]
		public Task SimpleWorkWithSyncContinueAsync<T>([NotNull] Func<Task<T>> delegateTask,
			[NotNull] Action<T> continueWith,
			[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegateTask == null)
			{
				throw new ArgumentNullException(nameof(delegateTask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return
				SimpleWorkInternal(CreateNewAsyncTask(delegateTask.Invoke),
					t => continueWith(t.Result.Result), taskName);
		}

		/// <summary>
		///     Runs the <paramref name="delegateTask" /> in a task and schedules the <paramref name="continueWith" /> in the
		///     Dispatcher
		/// </summary>
		/// <param name="delegateTask"></param>
		/// <param name="continueWith"></param>
		/// <param name="taskName"></param>
		/// <returns>The created and running Task</returns>
		[NotNull]
		[PublicAPI]
		public Task SimpleWorkWithSyncContinue([NotNull] Action delegateTask, [NotNull] Action continueWith,
			[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegateTask == null)
			{
				throw new ArgumentNullException(nameof(delegateTask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return SimpleWorkInternal(CreateNewTask(delegateTask.Invoke), s =>
				ThreadSaveAction(continueWith), taskName);
		}

		/// <summary>
		///     Runs the <paramref name="delegateTask" /> in a task. Does not set the IsWorking Flag
		/// </summary>
		/// <param name="delegateTask"></param>
		/// <param name="taskName"></param>
		/// <returns></returns>
		[NotNull]
		[PublicAPI]
		public Task BackgroundSimpleWork([NotNull] Action delegateTask,
			[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegateTask == null)
			{
				throw new ArgumentNullException(nameof(delegateTask));
			}

			return SimpleWorkInternal(CreateNewTask(delegateTask.Invoke), null,
				taskName, false);
		}

		/// <summary>
		///     Runs the <paramref name="delegateTask" /> in a task. Does not set the IsWorking Flag
		/// </summary>
		/// <param name="delegateTask"></param>
		/// <param name="continueWith"></param>
		/// <param name="taskName"></param>
		/// <returns></returns>
		[NotNull]
		[PublicAPI]
		public Task BackgroundSimpleWork<T>([NotNull] Func<T> delegateTask, [NotNull] Action<T> continueWith,
			[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegateTask == null)
			{
				throw new ArgumentNullException(nameof(delegateTask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return SimpleWorkInternal(CreateNewAsyncTask(delegateTask.Invoke),
				t => continueWith(t.Result), taskName, false);
		}

		/// <summary>
		///     Runs the <paramref name="delegateTask" /> in a task. Does not set the IsWorking Flag
		/// </summary>
		/// <param name="delegateTask"></param>
		/// <param name="continueWith"></param>
		/// <param name="taskName"></param>
		/// <returns></returns>
		[NotNull]
		[PublicAPI]
		public Task BackgroundSimpleWorkAsync<T>([NotNull] Func<Task<T>> delegateTask, [NotNull] Action<T> continueWith,
			[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegateTask == null)
			{
				throw new ArgumentNullException(nameof(delegateTask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return
				SimpleWorkInternal(CreateNewAsyncTask(delegateTask.Invoke),
					t => continueWith(t.Result.Result), taskName, false);
		}

		/// <summary>
		///     Runs the <paramref name="delegateTask" />
		/// </summary>
		/// <param name="delegateTask"></param>
		/// <param name="taskName"></param>
		/// <returns></returns>
		[NotNull]
		[PublicAPI]
		public Task SimpleWork([NotNull] Action delegateTask, [CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegateTask == null)
			{
				throw new ArgumentNullException(nameof(delegateTask));
			}

			return SimpleWorkInternal(CreateNewTask(delegateTask.Invoke), null,
				taskName);
		}

		/// <summary>
		///     Runs the <paramref name="delegateTask" /> and executes the continueWith after that
		/// </summary>
		/// <param name="delegateTask"></param>
		/// <param name="continueWith"></param>
		/// <param name="taskName"></param>
		/// <returns></returns>
		[NotNull]
		[PublicAPI]
		public Task SimpleWork([NotNull] Action delegateTask, [NotNull] Action continueWith,
			[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegateTask == null)
			{
				throw new ArgumentNullException(nameof(delegateTask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return SimpleWorkInternal(CreateNewTask(delegateTask.Invoke),
				task => continueWith(), taskName);
		}

		/// <summary>
		///     Runs the <paramref name="delegateTask" /> and executes the continueWith after that
		/// </summary>
		/// <param name="delegateTask"></param>
		/// <param name="continueWith"></param>
		/// <param name="taskName"></param>
		/// <returns></returns>
		[NotNull]
		[PublicAPI]
		public Task SimpleWork<T>([NotNull] Func<T> delegateTask, [NotNull] Action<T> continueWith,
			[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegateTask == null)
			{
				throw new ArgumentNullException(nameof(delegateTask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return SimpleWorkInternal(CreateNewAsyncTask(delegateTask.Invoke),
				s => continueWith(s.Result),
				taskName);
		}

		/// <summary>
		///     Runs the <paramref name="delegateTask" /> and executes the continueWith after that
		/// </summary>
		/// <param name="delegateTask"></param>
		/// <param name="continueWith"></param>
		/// <param name="taskName"></param>
		/// <returns></returns>
		[NotNull]
		[PublicAPI]
		public Task SimpleWorkAsync([NotNull] Func<Task> delegateTask, [NotNull] Action continueWith,
			[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegateTask == null)
			{
				throw new ArgumentNullException(nameof(delegateTask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return
				SimpleWorkInternal(CreateNewTask(() => delegateTask.Invoke().Wait()),
					task => continueWith(),
					taskName);
		}

		/// <summary>
		///     Runs the <paramref name="delegateTask" /> and executes the continueWith after that
		/// </summary>
		/// <param name="delegateTask"></param>
		/// <param name="taskName"></param>
		/// <returns></returns>
		[NotNull]
		[PublicAPI]
		public Task SimpleWorkAsync([NotNull] Func<Task> delegateTask,
			[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegateTask == null)
			{
				throw new ArgumentNullException(nameof(delegateTask));
			}

			return
				SimpleWorkInternal(CreateNewTask(() => delegateTask.Invoke().Wait()),
					null,
					taskName);
		}

		/// <summary>
		///     Runs the <paramref name="delegateTask" /> and executes the continueWith after that
		/// </summary>
		/// <param name="delegateTask"></param>
		/// <param name="continueWith"></param>
		/// <param name="taskName"></param>
		/// <returns></returns>
		[NotNull]
		[PublicAPI]
		public Task SimpleWorkAsync<T>([NotNull] Func<Task<T>> delegateTask, [NotNull] Action<T> continueWith,
			[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegateTask == null)
			{
				throw new ArgumentNullException(nameof(delegateTask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return
				SimpleWorkInternal(CreateNewAsyncTask(delegateTask.Invoke),
					s => continueWith(s.Result.Result),
					taskName);
		}

		/// <summary>
		///     Creates a Background Task
		/// </summary>
		/// <param name="delegateTask">The Delegate that should be executed async.</param>
		/// <param name="continueWith">
		///     The Delegate that should be executed when <paramref name="delegateTask" /> is done. Must
		///     accept an Task as first argument
		/// </param>
		/// <param name="taskName"></param>
		/// <returns></returns>
		[NotNull]
		[PublicAPI]
		public Task SimpleWork([NotNull] Delegate delegateTask, [NotNull] Delegate continueWith,
			[CallerMemberName] string taskName = AnonymousTask)
		{
			if (delegateTask == null)
			{
				throw new ArgumentNullException(nameof(delegateTask));
			}

			if (continueWith == null)
			{
				throw new ArgumentNullException(nameof(continueWith));
			}

			return SimpleWorkInternal(CreateNewAsyncTask(async () =>
			{
				var dynamicInvokeResult = delegateTask.DynamicInvoke();
				if (dynamicInvokeResult is Task)
				{
					await (dynamicInvokeResult as Task);
				}
			}), task => continueWith.DynamicInvoke());
		}
		
		/// <summary>
		///     Runs the <paramref name="task" /> and executes the continueWith after that
		/// </summary>
		/// <param name="task">An Started or Not started task</param>
		/// <param name="continueWith"></param>
		/// <param name="taskName"></param>
		/// <param name="setWorking"></param>
		/// <returns></returns>
		[NotNull]
		[PublicAPI]
		public Task SimpleWork([NotNull] Task task, [NotNull] Delegate continueWith,
			[CallerMemberName] string taskName = AnonymousTask,
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
		[NotNull]
		[PublicAPI]
		public Task SimpleWork([NotNull] Task task, [NotNull] Action continueWith,
			[CallerMemberName] string taskName = AnonymousTask,
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

		/// <summary>
		///     Begins a scope where multiple task producing actions can be started and awaited together.
		///     Call it in a using and the dispose will wait until all actions started inside it will be complete
		/// </summary>
		/// <returns></returns>
		[NotNull]
		[MustUseReturnValue]
		[PublicAPI]
		public AwaitMultiple BeginScope()
		{
			return new AwaitMultiple(this);
		}

		/// <summary>
		///		This event will be fired each time an new SimpleWork is created
		/// </summary>
		public event EventHandler<Task> TaskCreated;

		/// <summary>
		///		Occurs when any task is executed.
		/// </summary>
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
			Action continueWithWrapper = () =>
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

			return continueWithWrapper;
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
		[NotNull]
		[PublicAPI]
		public Task SimpleWork([NotNull] Task task, [CallerMemberName] string taskName = AnonymousTask)
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
		[NotNull]
		[PublicAPI]
		public Task SimpleWork([NotNull] Task task, bool setWorking, [CallerMemberName] string taskName = AnonymousTask)
		{
			if (task == null)
			{
				throw new ArgumentNullException(nameof(task));
			}

			return SimpleWorkInternal(task, null, taskName, setWorking);
		}

		/// <summary>
		///		Raises the TaskCreated Event
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnTaskCreated([NotNull] Task e)
		{
			TaskCreated?.Invoke(this, e);
		}

		/// <summary>
		///		Raises the TaskDone event
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnTaskDone([NotNull] Task e)
		{
			TaskDone?.Invoke(this, e);
		}

		/// <summary>
		///		The scope for Awaiting multiple Tasks
		/// </summary>
		/// <seealso cref="System.Collections.Generic.IEnumerable{System.Threading.Tasks.Task}" />
		/// <seealso cref="System.IDisposable" />
		public class AwaitMultiple : IEnumerable<Task>, IDisposable
		{
			private readonly AsyncViewModelBase _sender;

			private readonly List<Task> _tasks;

			/// <summary>
			/// Initializes a new instance of the <see cref="AwaitMultiple"/> class.
			/// </summary>
			/// <param name="sender">The sender.</param>
			public AwaitMultiple(AsyncViewModelBase sender)
			{
				_sender = sender;
				_tasks = new List<Task>();
				_sender.TaskCreated += _sender_TaskCreated;
			}

			public void Dispose()
			{
				_sender.TaskCreated -= _sender_TaskCreated;
			}

			/// <inheritdoc />
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

			/// <summary>
			///		Gets an Awaiter for awaiting all captured Tasks
			/// </summary>
			/// <returns></returns>
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

		/// <summary>
		/// Gets a value indicating whether this instance is working task.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is working task; otherwise, <c>false</c>.
		/// </value>
		public bool IsWorkingTask
		{
			get { return TaskList.Any(); }
		}

		#endregion
	}
}