using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.WPFBase.MVVM.ViewModel;
using NUnit.Framework;

namespace JPB.WpfBase.Tests.MVVM.ViewModel.ThreadSaveCollection
{
	[TestFixture]
	public class ThreadSaveObservableCollectionTests
	{
		[Explicit]
		[Test]
		public void Playground()
		{
			var tsoc = new ThreadSaveObservableCollection<object>();
			tsoc.Add(new object());
			tsoc.Add("test");
			tsoc.Add(0x00);

			Console.WriteLine();
		}
	}
}
