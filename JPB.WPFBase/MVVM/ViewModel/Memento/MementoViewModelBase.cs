#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
	[IgnoreMemento]
	public class MementoViewModelBase : AsyncViewModelBase, IDisposable
	{
		internal ConcurrentDictionary<string, IMementoValueHolder> MementoDataStore => _mementoDataStore;

		internal static IDictionary<Type, string[]> MementoIgnores { get; set; }

		static MementoViewModelBase()
		{
			MementoIgnores = new ConcurrentDictionary<Type, string[]>();
		}

		[NonSerialized]
		private MementoController _mementoController;

		[NonSerialized]
		private readonly ConcurrentDictionary<string, IMementoValueHolder> _mementoDataStore;

		public MementoViewModelBase()
		{
			_mementoDataStore = new ConcurrentDictionary<string, IMementoValueHolder>();
			MementoOptions = MementoOptions.Default;
			CheckForIgnores(GetType());
		}

		private void CheckForIgnores(Type type)
		{
			while (true)
			{
				if (MementoIgnores.ContainsKey(type))
				{
					foreach (var mementoIgnore in MementoIgnores[type])
					{
						MementoControl.Blacklist(mementoIgnore);
					}
				}
				else
				{
					var properties = type.GetProperties();

					if (type.GetCustomAttributes(false).Any(e => e is IgnoreMementoAttribute))
					{
						foreach (var propertyName in MementoIgnores[type] = properties.Select(e => e.Name).ToArray())
						{
							MementoControl.Blacklist(propertyName);
						}
					}
					else
					{
						var ignores = properties.Where(e => e.GetCustomAttribute(typeof(IgnoreMementoAttribute)) != null)
						                        .Select(e => e.Name).ToArray();
						foreach (var propertyName in MementoIgnores[type] = ignores)
						{
							MementoControl.Blacklist(propertyName);
						}
					}
				}
				if (type.BaseType != null && type.BaseType.Assembly != typeof(string).Assembly 
				                          && type.BaseType.Assembly != typeof(MementoController).Assembly)
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
		public IReadOnlyDictionary<string, IMementoValueProducer> MementoData
		{
			get { return new ReadOnlyDictionary<string, IMementoValueProducer>(MementoDataStore.ToDictionary(f => f.Key, f => f.Value as IMementoValueProducer)); }
		}

		/// <summary>
		///     Starts the Listening to the INotifyPropertyChanged event
		/// </summary>
		public void StartCapture()
		{
			WeakEventManager<MementoViewModelBase, PropertyChangedEventArgs>.AddHandler(this, nameof(INotifyPropertyChanged.PropertyChanged), MementoViewModelBase_PropertyChanged);
			WeakEventManager<MementoViewModelBase, PropertyChangingEventArgs>.AddHandler(this, nameof(INotifyPropertyChanging.PropertyChanging),
			MementoViewModelBase_PropertyChangeing);
			CaptureByINotifyPropertyChanged = true;
		}

		/// <summary>
		///     Stops the Listening to the INotifyPropertyChanged event
		/// </summary>
		public void StopCapture()
		{
			WeakEventManager<MementoViewModelBase, PropertyChangedEventArgs>.RemoveHandler(this, nameof(INotifyPropertyChanged.PropertyChanged), MementoViewModelBase_PropertyChanged);
			WeakEventManager<MementoViewModelBase, PropertyChangingEventArgs>.RemoveHandler(this, nameof(INotifyPropertyChanging.PropertyChanging),
			MementoViewModelBase_PropertyChangeing);
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

		private void MementoViewModelBase_PropertyChangeing(object send, PropertyChangingEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(e.PropertyName))
			{
				return;
			}

			var mementoValueProducer = GetOrAddMemento(e.PropertyName);
			if (mementoValueProducer == null)
			{
				return;
			}

			var value = mementoValueProducer.GetValue(this);
			if (value is INotifyCollectionChanged)
			{
				(value as INotifyCollectionChanged).CollectionChanged -= OnMomentTrackedCollectionChanged;
			}
		}

		protected internal void OnMomentTrackedCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{

		}

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

		internal IMementoValueHolder GetOrAddMemento(string propertyName)
		{
			if (!DoNotSetMoment)
			{
				return MementoDataStore.GetOrAdd(propertyName, f => new MementoValueProducer(f, MementoOptions));
			}

			return null;
		}
	}
}