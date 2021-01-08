using System;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using NUnit.Framework;

namespace JPB.WpfBase.Tests.MVVM.Commanding
{
	[TestFixture]
	public class DelegateCommandTypedTests
	{
		[Test]
		public void TestDelegateWithMatchingArgument()
		{
			bool executed = false;
			var command = new DelegateCommand<Guid>(() => executed = true);
			Assert.That(executed, Is.False);
			command.Execute(Guid.NewGuid());
			Assert.That(executed, Is.True);
		}

		[Test]
		public void TestDelegateWithNotMatchingArgument()
		{
			bool executed = false;
			var command = new DelegateCommand<Guid>(() => executed = true);
			Assert.That(executed, Is.False);
			Assert.That(() =>
			{
				command.Execute();
			}, Throws.Nothing);
			Assert.That(executed, Is.True);
			executed = false;
			Assert.That(() =>
			{
				command.Execute(1322);
			}, Throws.Nothing);
			Assert.That(executed, Is.True);
		}

		[Test]
		public void TestDelegateWithMatchingArgumentAndCanExecute()
		{
			bool executed = false;
			var command = new DelegateCommand<Guid>((f) => executed = true, (f) => true);
			Assert.That(executed, Is.False);
			command.Execute(Guid.NewGuid());
			Assert.That(executed, Is.True);
		}

		[Test]
		public void TestDelegateWithNotMatchingArgumentAndCanExecute()
		{
			bool executed = false;
			var command = new DelegateCommand<Guid>((f) => executed = true, f => true);
			Assert.That(executed, Is.False);

			Assert.That(() =>
			{
				command.Execute("");
			}, Throws.Exception.TypeOf<InvalidOperationException>());
			Assert.That(executed, Is.False);
		}
	}
}