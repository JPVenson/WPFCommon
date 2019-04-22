using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using JPB.WPFBase.MVVM.ViewModel;
using Microsoft.Win32.SafeHandles;
using NUnit.Framework;

namespace JPB.WpfBase.Tests.MVVM.ViewModel.AsyncViewModel
{
	[TestFixture]
	public class AsyncViewModelBaseTests
	{
		public AsyncViewModelBaseTests()
		{

		}

		[Test]
		public void TestIsWorking()
		{
			var vm = new AsyncTestViewModelBase();
			AssertVmIs(vm, false);
			using (var isWorkingChanged = new SendPropertyChangeHandler(vm, nameof(AsyncViewModelBase.IsWorking)))
			using (var isNotWorkingChanged = new SendPropertyChangeHandler(vm, nameof(AsyncViewModelBase.IsNotWorking)))
			using (var isWorkingTaskChanged = new SendPropertyChangeHandler(vm, nameof(AsyncViewModelBase.IsWorkingTask)))
			{
				vm.SimpleWork(() => Thread.Sleep(1000));

				AssertVmIs(vm, true);
				Assert.That(isWorkingChanged.TriggerCount, Is.EqualTo(1));
				Assert.That(isNotWorkingChanged.TriggerCount, Is.EqualTo(1));
				Assert.That(isWorkingTaskChanged.TriggerCount, Is.EqualTo(1));
				Thread.Sleep(1000);

				AssertVmIs(vm, false);
				Assert.That(isWorkingChanged.TriggerCount, Is.EqualTo(2));
				Assert.That(isNotWorkingChanged.TriggerCount, Is.EqualTo(2));
				Assert.That(isWorkingTaskChanged.TriggerCount, Is.EqualTo(2));
			}
		}

		[Test]
		public void TestCanExecuteCondition()
		{
			var vm = new AsyncTestViewModelBase();
			vm.SimpleWork(TestExecute, null, true, nameof(TestExecute));
			Assert.That(vm.CheckCanExecuteCondition(nameof(CanTestExecute)), Is.False);
			Thread.Sleep(1000);
			Assert.That(vm.CheckCanExecuteCondition(nameof(CanTestExecute)), Is.True);
		}

		private void CanTestExecute()
		{

		}

		private void TestExecute()
		{
			Thread.Sleep(1000);
		}

		public Task AttachTaskEndedHandler(AsyncViewModelBase vm)
		{
			var waiter = new ManualResetEventSlim();

			void OnVmOnTaskDone(object sender, Task task)
			{
				waiter.Set();
			}

			vm.TaskDone += OnVmOnTaskDone;
			return WaitOneAsync(waiter.WaitHandle, () =>
			{
				vm.TaskDone -= OnVmOnTaskDone;
			});
		}

		public static Task WaitOneAsync(WaitHandle waitHandle, Action then)
		{
			if (waitHandle == null)
				throw new ArgumentNullException("waitHandle");

			var tcs = new TaskCompletionSource<bool>();
			var rwh = ThreadPool.RegisterWaitForSingleObject(waitHandle,
				delegate { tcs.TrySetResult(true); }, null, -1, true);
			var t = tcs.Task;
			t.ContinueWith((antecedent) =>
			{
				then();
				return rwh.Unregister(null);
			});
			return t;
		}

		/// <summary>
		///		Schedules an Existing task.
		///		That has an Result.
		///		Schedules an Continue.
		///		The Continue is ThreadSaveAction
		///		And has the result of the Task
		/// </summary>
		[Test]
		public async Task TestSimpleWorkWithSyncContinue_TaskWithResult_ContinueWithResult()
		{
			var vm = new AsyncTestViewModelBase();
			var waitForComplete = AttachTaskEndedHandler(vm);
			var continueExecuted = false;
			var resultCode = Guid.NewGuid();
			var resultCodes = new Dictionary<string, bool>();
			var task = Task.Run<Guid>(() =>
			{
				Thread.Sleep(1000);
				return resultCode;
			});

			var continueWith = new Action<Guid>(f =>
			{
				continueExecuted = true;
				resultCodes["Argument is same"] = f.Equals(resultCode);
				resultCodes["Is in Bootstrapper Dispatcher"] = WpfBootstrapper.App.CheckAccess();
				resultCodes["Bootstrapper Dispatcher is Current"] =
					WpfBootstrapper.App.Dispatcher == Dispatcher.CurrentDispatcher;
			});
			AssertVmIs(vm, false);
			var result = vm.SimpleWorkAsync(task, continueWith, true);
			AssertVmIs(vm, true);
			Assert.That(result, Is.EqualTo(task));
			await result;
			AssertVmIs(vm, true);
			await waitForComplete;
			AssertVmIs(vm, false);

			Assert.That(continueExecuted, Is.True);
			foreach (var code in resultCodes)
			{
				Assert.That(code.Value, Is.True, code.Key);
			}
		}

		/// <summary>
		///		Schedules an Existing task.
		///		Schedules an Continue.
		///		The Continue is ThreadSaveAction
		/// </summary>
		[Test]
		public async Task TestSimpleWorkWithSyncContinue_Task_Continue()
		{
			var vm = new AsyncTestViewModelBase();
			var waitForComplete = AttachTaskEndedHandler(vm);
			var continueExecuted = false;
			var resultCodes = new Dictionary<string, bool>();
			var task = Task.Run(() =>
			{
				Thread.Sleep(1000);
			});

			var continueWith = new Action(() =>
			{
				continueExecuted = true;
				resultCodes["Is in Bootstrapper Dispatcher"] = WpfBootstrapper.App.CheckAccess();
				resultCodes["Bootstrapper Dispatcher is Current"] =
					WpfBootstrapper.App.Dispatcher == Dispatcher.CurrentDispatcher;
			});
			AssertVmIs(vm, false);
			var result = vm.SimpleWorkAsync(task, continueWith, true);
			AssertVmIs(vm, true);
			Assert.That(result, Is.EqualTo(task));
			await result;
			AssertVmIs(vm, true);
			await waitForComplete;
			AssertVmIs(vm, false);

			Assert.That(continueExecuted, Is.True);
			foreach (var code in resultCodes)
			{
				Assert.That(code.Value, Is.True, code.Key);
			}
		}

		/// <summary>
		///		Schedules an Function.
		///		That has an Result.
		///		Schedules an Continue.
		///		The Continue is ThreadSaveAction
		///		And has the result of the Function
		/// </summary>
		[Test]
		public async Task TestSimpleWorkWithSyncContinue_FunctionWithResult_ContinueWithResult()
		{
			var vm = new AsyncTestViewModelBase();
			var waitForComplete = AttachTaskEndedHandler(vm);
			var continueExecuted = false;
			var resultCode = Guid.NewGuid();
			var resultCodes = new Dictionary<string, bool>();
			var task = new Func<Guid>(() =>
			{
				Thread.Sleep(1000);
				return resultCode;
			});

			var continueWith = new Action<Guid>((f) =>
			{
				continueExecuted = true;
				resultCodes["Argument is same"] = f.Equals(resultCode);
				resultCodes["Is in Bootstrapper Dispatcher"] = WpfBootstrapper.App.CheckAccess();
				resultCodes["Bootstrapper Dispatcher is Current"] =
					WpfBootstrapper.App.Dispatcher == Dispatcher.CurrentDispatcher;
			});

			AssertVmIs(vm, false);
			var result = vm.SimpleWork(task, continueWith, true);
			AssertVmIs(vm, true);
			await result;
			AssertVmIs(vm, true);
			await waitForComplete;
			AssertVmIs(vm, false);

			Assert.That(continueExecuted, Is.True);
			foreach (var code in resultCodes)
			{
				Assert.That(code.Value, Is.True, code.Key);
			}
		}
		/// <summary>
		///		Schedules an Function.
		///		That is Async.
		///		That has an Result.
		///		Schedules an Continue.
		///		The Continue is ThreadSaveAction
		/// </summary>
		[Test]
		public async Task TestSimpleWorkWithSyncContinue_AsyncFunction_Continue()
		{
			var vm = new AsyncTestViewModelBase();
			var waitForComplete = AttachTaskEndedHandler(vm);
			var continueExecuted = false;
			var resultCodes = new Dictionary<string, bool>();
			Func<Task> task = async () =>
			{
				await Task.Delay(1000);
			};

			var continueWith = new Action(() =>
			{
				continueExecuted = true;
				resultCodes["Is in Bootstrapper Dispatcher"] = WpfBootstrapper.App.CheckAccess();
				resultCodes["Bootstrapper Dispatcher is Current"] =
					WpfBootstrapper.App.Dispatcher == Dispatcher.CurrentDispatcher;
			});

			AssertVmIs(vm, false);
			var result = vm.SimpleWorkAsync(task, continueWith, true);
			AssertVmIs(vm, true);
			await result;
			AssertVmIs(vm, true);
			await waitForComplete;
			AssertVmIs(vm, false);

			Assert.That(continueExecuted, Is.True);
			foreach (var code in resultCodes)
			{
				Assert.That(code.Value, Is.True, code.Key);
			}
		}

		/// <summary>
		///		Schedules an Function.
		///		That is Async.
		///		Schedules an Continue.
		///		The Continue is ThreadSaveAction
		///		And has the result of the Function
		/// </summary>
		[Test]
		public async Task TestSimpleWorkWithSyncContinue_AsyncFunctionWithResult_ContinueWithResult()
		{
			var vm = new AsyncTestViewModelBase();
			var waitForComplete = AttachTaskEndedHandler(vm);
			var continueExecuted = false;
			var resultCode = Guid.NewGuid();
			var resultCodes = new Dictionary<string, bool>();
			Func<Task<Guid>> task = async () =>
			{
				await Task.Delay(1000);
				return resultCode;
			};

			var continueWith = new Action<Guid>((f) =>
			{
				continueExecuted = true;
				resultCodes["Argument is same"] = f.Equals(resultCode);
				resultCodes["Is in Bootstrapper Dispatcher"] = WpfBootstrapper.App.CheckAccess();
				resultCodes["Bootstrapper Dispatcher is Current"] =
					WpfBootstrapper.App.Dispatcher == Dispatcher.CurrentDispatcher;
			});

			AssertVmIs(vm, false);
			var result = vm.SimpleWorkAsync(task, continueWith, true);
			AssertVmIs(vm, true);
			await result;
			AssertVmIs(vm, true);
			await waitForComplete;
			AssertVmIs(vm, false);

			Assert.That(continueExecuted, Is.True);
			foreach (var code in resultCodes)
			{
				Assert.That(code.Value, Is.True, code.Key);
			}
		}

		/// <summary>
		///		Schedules an Action.
		///		Schedules an Continue.
		///		The Continue is ThreadSaveAction
		/// </summary>
		[Test]
		public async Task TestSimpleWorkWithSyncContinue_Action_Continue()
		{
			var vm = new AsyncTestViewModelBase();
			var waitForComplete = AttachTaskEndedHandler(vm);
			var continueExecuted = false;
			var resultCodes = new Dictionary<string, bool>();
			Action task = () =>
			{
				Thread.Sleep(1000);
			};

			var continueWith = new Action(() =>
			{
				continueExecuted = true;
				resultCodes["Is in Bootstrapper Dispatcher"] = WpfBootstrapper.App.CheckAccess();
				resultCodes["Bootstrapper Dispatcher is Current"] =
					WpfBootstrapper.App.Dispatcher == Dispatcher.CurrentDispatcher;
			});

			AssertVmIs(vm, false);
			var result = vm.SimpleWork(task, continueWith, true);
			AssertVmIs(vm, true);
			await result;
			AssertVmIs(vm, true);
			await waitForComplete;
			AssertVmIs(vm, false);

			Assert.That(continueExecuted, Is.True);
			foreach (var code in resultCodes)
			{
				Assert.That(code.Value, Is.True, code.Key);
			}
		}

		private void AssertVmIs(AsyncViewModelBase vm, bool working)
		{
			Assert.That(vm.IsWorking, Is.EqualTo(working));
			Assert.That(vm.IsWorkingTask, Is.EqualTo(working));
			Assert.That(vm.IsNotWorking, Is.EqualTo(!working));
		}
	}

	public class SendPropertyChangeHandler : IDisposable
	{
		private readonly INotifyPropertyChanged _sender;

		public SendPropertyChangeHandler(INotifyPropertyChanged sender, string propertyName)
		{
			PropertyName = propertyName;
			_sender = sender;
			sender.PropertyChanged += Sender_PropertyChanged;
		}

		private void Sender_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == PropertyName)
			{
				TriggerCount++;
			}
		}

		public string PropertyName { get; private set; }
		public int TriggerCount { get; private set; }

		public void Reset()
		{
			TriggerCount = 0;
		}

		public void Dispose()
		{
			_sender.PropertyChanged -= Sender_PropertyChanged;
		}
	}

	public class AsyncTestViewModelBase : AsyncViewModelBase
	{
		public AsyncTestViewModelBase(Dispatcher dispatcher) : base(dispatcher)
		{

		}

		public AsyncTestViewModelBase()
		{

		}
	}
}
