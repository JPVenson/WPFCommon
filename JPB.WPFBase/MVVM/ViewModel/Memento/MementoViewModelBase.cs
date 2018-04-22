#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using JPB.WPFBase.MVVM.ViewModel.Memento.Attributes;

#endregion

namespace JPB.WPFBase.MVVM.ViewModel.Memento
{
	/// <summary>
	///     Uses the Memento Pattern to Store and revert all INotifyPropertyChanged/ing properties
	/// </summary>
	public class MementoViewModelBase : AsyncViewModelBase, IDisposable
	{
		internal ConcurrentDictionary<string, MementoValueProducer> MementoDataStore => _mementoDataStore;

		[NonSerialized]
		private MementoController _mementoController;

		[NonSerialized]
		private readonly ConcurrentDictionary<string, MementoValueProducer> _mementoDataStore;

		public MementoViewModelBase()
		{
			_mementoDataStore = new ConcurrentDictionary<string, MementoValueProducer>();
			MementoOptions = MementoOptions.Default;
			CheckForIgnores(GetType());
		}

		private void CheckForIgnores(Type type)
		{
			while (true)
			{
				if (type.GetCustomAttributes(false).Any(e => e is IgnoreMementoAttribute))
				{
					foreach (var propertyInfo in type.GetProperties())
					{
						MementoControl.Blacklist(propertyInfo.Name);
					}
				}

				if (type.BaseType != null && type.BaseType.Assembly != typeof(string).Assembly)
				{
					type = type.BaseType;
					continue;
				}

				break;
			}
		}

		/// <summary>
		///		Creates a new <c>AsyncViewModelBase</c> with the given Dispatcher
		/// </summary>
		/// <param name="disp"></param>
		protected MementoViewModelBase(Dispatcher disp)
				: base(disp)
		{
			Init();
		}

		internal override void DispShutdownStarted(object sender, EventArgs e)
		{
			base.DispShutdownStarted(sender, e);
			Dispose();
		}

		/// <summary>
		///		Returns a Controller that can be used to control the current memento of a Property or all properties
		/// </summary>
		/// <returns></returns>
		public MementoController MementoControl
		{
			get
			{
				return _mementoController ?? (_mementoController = new MementoController(this));
			}
		}

		/// <summary>
		///     The options set for this instance
		/// </summary>
		public MementoOptions MementoOptions { get; set; }

		/// <summary>
		///     If set the <code>MementoViewModelBase</code> listens to the INotifyPropertyChanged event
		/// </summary>
		public bool CaptureByINotifyPropertyChanged { get; private set; }

		/// <summary>
		///     The Current known Memento Data for all Properties
		/// </summary>
		public IReadOnlyDictionary<string, MementoValueProducer> MementoData
		{
			get { return new ReadOnlyDictionary<string, MementoValueProducer>(MementoDataStore); }
		}

		/// <summary>
		///     Starts the Listening to the INotifyPropertyChanged event
		/// </summary>
		public void StartCapture()
		{
			WeakEventManager<MementoViewModelBase, PropertyChangedEventArgs>.AddHandler(this, nameof(INotifyPropertyChanged.PropertyChanged), MementoViewModelBase_PropertyChanged);
			CaptureByINotifyPropertyChanged = true;
		}

		/// <summary>
		///     Stops the Listening to the INotifyPropertyChanged event
		/// </summary>
		public void StopCapture()
		{
			WeakEventManager<MementoViewModelBase, PropertyChangedEventArgs>.RemoveHandler(this, nameof(INotifyPropertyChanged.PropertyChanged), MementoViewModelBase_PropertyChanged);
			CaptureByINotifyPropertyChanged = false;
		}

		/// <summary>
		///     If not Overwritten, returns a DataStamp objects that controls how the data is stored
		/// </summary>
		/// <returns></returns>
		protected virtual IMementoDataStamp CreateDataStemp()
		{
			if (MementoOptions.WeakData)
			{
				return new WeakMementoDataStamp();
			}

			return new MementoDataStamp();
		}

		/// <summary>
		///		Used to Control the GoInHistory feature. If we are currently switching the History, do not emit a moment
		/// </summary>
		[ThreadStatic]
		[NonSerialized]
		internal static bool DoNotSetMoment;

		private void MementoViewModelBase_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(e.PropertyName))
			{
				foreach (var mementoValueProducer in MementoDataStore)
				{
					mementoValueProducer.Value.TryAdd(this, CreateDataStemp());
				}

				return;
			}

			GetOrAddMemento(e.PropertyName)?
					.TryAdd(this, CreateDataStemp());
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}

			StopCapture();
			_mementoController?.Forget();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public MementoValueProducer GetOrAddMemento(string propertyName)
		{
			if (!DoNotSetMoment)
			{
				return MementoDataStore.GetOrAdd(propertyName, f => new MementoValueProducer(f, MementoOptions));
			}

			return null;
		}
	}
}