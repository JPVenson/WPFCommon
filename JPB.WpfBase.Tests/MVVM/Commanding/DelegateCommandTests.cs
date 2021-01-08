using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JPB.WpfBase.Tests.MVVM.ViewModel.AsyncViewModel;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace JPB.WpfBase.Tests.MVVM.Commanding
{
	[TestFixture]
	public class DelegateCommandTests
	{
		[Test]
		public void TestCommand()
		{
			var executed = false;
			var command = new DelegateCommand(() => { executed = true; });
			Assert.That(executed, Is.False);
			command.Execute();
			Assert.That(executed, Is.True);
		}

		[Test]
		public void TestCommandWithArgument()
		{
			var param = Guid.NewGuid();

			var executed = false;
			var command = new DelegateCommand((argument) =>
			{
				executed = true;
				Assert.That(argument, Is.EqualTo(param));
			});
			Assert.That(executed, Is.False);
			command.Execute(param);
			Assert.That(executed, Is.True);
		}

		[Test]
		public void TestCommandWithCanExecute()
		{
			var executed = false;
			var enable = false;
			var command = new DelegateCommand(() => { executed = true; }, () => enable);
			Assert.That(executed, Is.False);
			Assert.That(command.CanExecute(), Is.False);
			enable = true; 
			Assert.That(command.CanExecute(), Is.True);
			command.Execute();
			Assert.That(executed, Is.True);
		}

		[Test]
		public void TestCommandWithCanExecuteThrowsException()
		{
			var executed = false;
			var enable = false;
			var command = new DelegateCommand(() => { executed = true; }, () => enable);
			Assert.That(executed, Is.False);
			Assert.That(() =>
			{
				command.Execute();
			}, Throws.Exception.TypeOf<InvalidOperationException>());
			Assert.That(executed, Is.False);
			enable = true;
			Assert.That(() => command.Execute(), Throws.Nothing);
			
			Assert.That(executed, Is.True);
		}
	}

	[TestFixture]
	public class StrictDelegateCommandTypedTests
	{
		[Test]
		public void TestDelegateWithMatchingArgument()
		{
			bool executed = false;
			var command = new StrictDelegateCommand<Guid>(() => executed = true);
			Assert.That(executed, Is.False);
			command.Execute(Guid.NewGuid());
			Assert.That(executed, Is.True);
		}

		[Test]
		public void TestDelegateWithNotMatchingArgument()
		{
			bool executed = false;
			var command = new StrictDelegateCommand<Guid>(() => executed = true);
			Assert.That(executed, Is.False);

			Assert.That(() =>
			{
				command.Execute("");
			}, Throws.Exception.TypeOf<InvalidOperationException>());
			Assert.That(executed, Is.False);
		}

		[Test]
		public void TestDelegateWithMatchingArgumentAndCanExecute()
		{
			bool executed = false;
			var command = new StrictDelegateCommand<Guid>((f) => executed = true, (f) => true);
			Assert.That(executed, Is.False);
			command.Execute(Guid.NewGuid());
			Assert.That(executed, Is.True);
		}

		[Test]
		public void TestDelegateWithNotMatchingArgumentAndCanExecute()
		{
			bool executed = false;
			var command = new StrictDelegateCommand<Guid>((f) => executed = true, f => true);
			Assert.That(executed, Is.False);

			Assert.That(() =>
			{
				command.Execute("");
			}, Throws.Exception.TypeOf<InvalidOperationException>());
			Assert.That(executed, Is.False);
		}
	}
	
	[TestFixture]
	public class AsyncDelegateCommandTypedTests
	{
		[Test]
		public async Task TestCommand()
		{
			var vm = new AsyncTestViewModelBase();
			var completeHandler = AsyncViewModelBaseTests.AttachTaskEndedHandler(vm);
			bool executed = false;
			var command = new AsyncDelegateCommand(vm, () =>
			{
				Thread.Sleep(1000);
				executed = true;
			});

			Assert.That(command.CanExecute(), Is.True);
			Assert.That(executed, Is.False); 
			Assert.That(vm.IsWorking, Is.False);
			command.Execute();
			Assert.That(executed, Is.False); 
			Assert.That(command.CanExecute(), Is.False);
			Assert.That(vm.IsWorking, Is.True);
			await command.Await();

			Assert.That(executed, Is.True);
			Assert.That(command.CanExecute(), Is.True);
			await completeHandler;
			Assert.That(vm.IsWorking, Is.False);
		}
	}
}
