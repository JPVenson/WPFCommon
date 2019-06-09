using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using JPB.WpfBase.Tests.MVVM.ViewModel.AsyncViewModel;
using JPB.WPFBase.Logger;
using JPB.WPFBase.MVVM.ViewModel;
using NUnit.Framework;

namespace JPB.WpfBase.Tests.MVVM.ViewModel.ThreadSaveCollection
{
	[TestFixture]
	public class ThreadSaveObservableCollectionTests
	{
		[Test]
		public void CheckICollectionView()
		{
			var collection = new ThreadSaveObservableCollection<object>();
			var collectionView = collection.ObtainView();
			collectionView.Filter = o => true;
			Assert.That(collectionView, Is.Not.Null);
			collectionView = collection.ObtainView("test");
			collectionView.Filter = o => true;
            Assert.That(collectionView, Is.Not.Null);
		}

		[Explicit]
		[Test]
		public void Playground()
		{
			//var monitor = new DispatcherStatusMonitor(Application.Current.Dispatcher, message =>
			//{
			//	Console.WriteLine(message.Time + ": " + message.Message);
			//});
			//monitor.Start();

			//var vm = new AsyncTestViewModelBase(Application.Current.Dispatcher);

			//long operationResult = 1;
			//for (int i = 0; i < 100000; i++)
			//{
			//	vm.ThreadSaveAction(() =>
			//	{
			//		var f = Math.Pow(2, operationResult++);
			//	});
			//}

			//Thread.Sleep(1000);
		}
	}
}
