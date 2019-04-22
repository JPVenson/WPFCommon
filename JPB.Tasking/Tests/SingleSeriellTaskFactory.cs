using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JPB.Tasking.TaskManagement.Threading;
using NUnit.Framework;

namespace JPB.Tasking.Tests
{
	[TestFixture]
	class SingleSeriellTaskFactoryTest
	{
		public SingleSeriellTaskFactoryTest()
		{
			
		}

		[Test]
		public void Create()
		{
			AutoResetEvent mre;
			using (var factory = new MultiTaskDispatcher(false))
			{
				mre = new AutoResetEvent(false);
				factory.TryAdd(() => { Thread.Sleep(100); }, 1);
				factory.TryAdd(() => { Thread.Sleep(100); }, 1);
				factory.TryAdd(() => { Thread.Sleep(100); }, 1);
				factory.TryAdd(() => { Thread.Sleep(100); }, 1);
				factory.TryAdd(() => { Thread.Sleep(100); }, 1);
				factory.TryAdd(() => { mre.Set(); }, 1);
				Assert.That(mre.WaitOne());
			}
		}
	}
}
