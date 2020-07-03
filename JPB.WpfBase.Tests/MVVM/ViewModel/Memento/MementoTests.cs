using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using JPB.WpfBase.Tests.MVVM.ViewModel.Memento.Models;
using JPB.WPFToolsAwesome.MVVM.ViewModel.Memento;
using JPB.WPFToolsAwesome.MVVM.ViewModel.Memento.Snapshots;
using NUnit.Framework;

namespace JPB.WpfBase.Tests.MVVM.ViewModel.Memento
{
	[TestFixture]
	public class MementoTests : ApplicationBaseTest
	{
		[Test]
		public void CreateViewModel()
		{
			var mementoViewModel = new MementoViewModel();
			mementoViewModel.Dispose();
			mementoViewModel = new MementoViewModel();
		}

		[Test]
		public void TestMementoSet()
		{
			var mementoViewModel = new MementoFullViewModel();
			mementoViewModel.StartCapture();

			Assert.That(mementoViewModel.Text, Is.Null);
			Assert.That(mementoViewModel.MementoData, Is.Empty);

			mementoViewModel.Text = "Test";

			Assert.That(mementoViewModel.MementoData, Is.Not.Empty);
			Assert.That(mementoViewModel.MementoData, Contains.Key(nameof(MementoFullViewModel.Text)));

			var mementoOfText = mementoViewModel.MementoData[nameof(MementoFullViewModel.Text)];

			Assert.That(mementoOfText, Is.Not.Null);
			Assert.That(mementoOfText.PropertyName, Is.EqualTo(nameof(MementoFullViewModel.Text)));
			Assert.That(mementoOfText.CurrentAge, Is.EqualTo(1));
			Assert.That(mementoOfText.CanGoInHistory(1), Is.False);
			Assert.That(mementoOfText.CanGoInHistory(-1), Is.False);
			Assert.That(mementoOfText.MementoDataStamps.Count(), Is.EqualTo(1));
			Assert.That(mementoOfText.MementoDataStamps.First().CanGetData(), Is.True);
			Assert.That(mementoOfText.MementoDataStamps.First().CanSetData("NewTest"), Is.False);
			Assert.That(mementoOfText.MementoDataStamps.First().GetData(), Is.EqualTo(mementoViewModel.Text));

			mementoViewModel.Text = "Test123";

			Assert.That(mementoOfText, Is.Not.Null);
			Assert.That(mementoOfText.PropertyName, Is.EqualTo(nameof(MementoFullViewModel.Text)));
			Assert.That(mementoOfText.CurrentAge, Is.EqualTo(2));
			Assert.That(mementoOfText.CanGoInHistory(1), Is.False);
			Assert.That(mementoOfText.CanGoInHistory(-1), Is.True);
			Assert.That(mementoOfText.MementoDataStamps.Count(), Is.EqualTo(2));
			Assert.That(mementoOfText.MementoDataStamps.First().CanGetData(), Is.True);
			Assert.That(mementoOfText.MementoDataStamps.First().CanSetData("NewTest"), Is.False);
			Assert.That(mementoOfText.MementoDataStamps.First().GetData(), Is.EqualTo(mementoViewModel.Text));
		}

		[Test]
		public void TestHistory()
		{
			var mementoViewModel = new MementoFullViewModel();
			mementoViewModel.StartCapture();

			mementoViewModel.Text = "Age1";
			mementoViewModel.Text = "Age2";
			mementoViewModel.Text = "Age3";

			Assert.That(mementoViewModel.MementoData, Is.Not.Empty);
			Assert.That(mementoViewModel.MementoData, Contains.Key(nameof(MementoFullViewModel.Text)));

			var mementoOfText = mementoViewModel.MementoData[nameof(MementoFullViewModel.Text)];

			Assert.That(mementoViewModel.MementoControl.CanGoInHistory(nameof(MementoFullViewModel.Text), -1), Is.True);

			Assert.That(mementoOfText.CurrentAge, Is.EqualTo(3));
			Assert.That(mementoOfText.MementoDataStamps.Count(), Is.EqualTo(3));

			mementoViewModel.MementoControl.GoInHistory(nameof(MementoFullViewModel.Text), -1);

			Assert.That(mementoOfText.CurrentAge, Is.EqualTo(2));
			Assert.That(mementoOfText.MementoDataStamps.Count(), Is.EqualTo(3));
			Assert.That(mementoViewModel.Text, Is.EqualTo("Age2"));

			mementoViewModel.Text = "Age3 1/2";
			Assert.That(mementoOfText.CurrentAge, Is.EqualTo(3));
			Assert.That(mementoOfText.MementoDataStamps.Count(), Is.EqualTo(3));
			Assert.That(mementoOfText.MementoDataStamps.Any(e => e.GetData().Equals("Age3")), Is.False);
			Assert.That(mementoOfText.MementoDataStamps.First().GetData(), Is.EqualTo(mementoViewModel.Text));
		}

		[Test]
		public void TestUiProxy()
		{
			var mementoViewModel = new MementoFullViewModel();
			mementoViewModel.StartCapture();

			mementoViewModel.Text = "Age1";
			mementoViewModel.Text = "Age2";
			mementoViewModel.Text = "Age3";

			var mementoProxy = mementoViewModel.MementoControl.Ui.Text as UiMementoController;
			Assert.That(mementoProxy, Is.Not.Null);
			Assert.That(object.ReferenceEquals(mementoViewModel.MementoControl.Ui.Text, mementoProxy), Is.True);
		}

		[Test]
		public void TestNofityKeywords()
		{
			var mementoViewModel = new MementoFullViewModel();
			mementoViewModel.StartCapture();

			mementoViewModel.Text = "Age1";
			mementoViewModel.Text = "Age2";
			mementoViewModel.Text = "Age3";

			mementoViewModel.SendPropertyChanged(null);
			mementoViewModel.SendPropertyChanged(string.Empty);
		}
		private readonly BinaryFormatter serializer = new BinaryFormatter();

		[Test]
		public void TestAdditonalData()
		{
			var mementoViewModel = new MementoFullWithDateTImeViewModel();
			mementoViewModel.StartCapture();

			mementoViewModel.Text = "It1-Age1";
			mementoViewModel.Text = "It1-Age2";
			mementoViewModel.Text = "It1-Age3";

			mementoViewModel.StopCapture();

			MementoObjectSnapshot snapshot;
			mementoViewModel.MementoControl.Snapshot(out snapshot);

			Assert.That(snapshot, Is.Not.Null);
			MemoryStream memoryStream = new MemoryStream();
			Assert.That(() =>
			{
				this.serializer.Serialize((Stream)memoryStream, (object)snapshot);
				memoryStream.Seek(0L, SeekOrigin.Begin);
				this.serializer.Deserialize((Stream)memoryStream);
			}, Throws.Nothing);


			Assert.That(snapshot, Is.BinarySerializable);
		}

		[Test]
		public void TestSnapshot()
		{
			var mementoViewModel = new MementoFullViewModel();
			mementoViewModel.StartCapture();

			mementoViewModel.Text = "It1-Age1";
			mementoViewModel.Text = "It1-Age2";
			mementoViewModel.Text = "It1-Age3";

			mementoViewModel.StopCapture();

			MementoObjectSnapshot snapshot;
			mementoViewModel.MementoControl.Snapshot(out snapshot);

			Assert.That(snapshot, Is.Not.Null);
			Assert.That(snapshot, Is.BinarySerializable);
			//Assert.That(snapshot, Is.XmlSerializable);
		}

		[Test]
		public void TransferSnapshot()
		{
			var mementoViewModel = new MementoFullViewModel();
			mementoViewModel.StartCapture();

			mementoViewModel.Text = "It1-Age1";
			mementoViewModel.Text = "It1-Age2";
			mementoViewModel.Text = "It1-Age3";

			mementoViewModel.StopCapture();

			MementoObjectSnapshot snapshot;
			mementoViewModel.MementoControl.Snapshot(out snapshot);

			var copy = new MementoFullViewModel();

			copy.MementoControl.ImportSnapshot(snapshot);

			Assert.That(copy.Text, Is.EqualTo(mementoViewModel.Text));

			//Assert.That(snapshot, Is.XmlSerializable);
		}
	}
}
