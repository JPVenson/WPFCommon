#region

using System.Collections.Concurrent;
using System.Collections.Generic;
using JPB.WPFBase.MVVM.ViewModel.Memento.Snapshots;

#endregion

namespace JPB.WPFBase.MVVM.ViewModel.Memento
{
	/// <summary>
	///     Defines how imported Moments should be processed
	/// </summary>
	public enum ImportFlags
	{
		///// <summary>
		/////		If you import an moments age it will overwrite the existing age
		///// </summary>
		//OverwriteExistingAges,
		///// <summary>
		/////		If you import an moments age it will be aged to fit after the existing age
		///// </summary>
		//ExistingFirst,
		///// <summary>
		/////		If you import an moments age existing ages will be aged to fit after the new age
		///// </summary>
		//ImportedFirst,

		/// <summary>
		///     All imported ages will be appended to the memento
		/// </summary>
		Append,

		/// <summary>
		///     All imported ages will be set before all existing moments
		/// </summary>
		Prefix
	}

	/// <summary>
	///     Defines methods for Controling the current momento of the ViewModel
	/// </summary>
	public class MementoController
	{
		private UiMementoProxy _uiMemento;

		internal MementoController(MementoViewModelBase mementoHost)
		{
			MementoHost = mementoHost;
		}

		internal MementoViewModelBase MementoHost { get; }

		/// <summary>
		///     When called will return an Proxy for Accessing the Memento functions for a Property. Call for the Property you want
		///     and then you will get a <see cref="UiMementoController" />
		/// </summary>
		/// <example>
		///     Ui.PropertyName.Forget
		/// </example>
		public dynamic Ui
		{
			get { return _uiMemento ?? (_uiMemento = new UiMementoProxy(this)); }
		}

		/// <summary>
		///     Ignores the Memento capture of a certain property
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public MementoController Blacklist(string propertyName)
		{
			if (!MementoHost.MementoDataStore.TryGetValue(propertyName, out var mementoValueProducer))
			{
				mementoValueProducer = MementoHost.GetOrAddMemento(propertyName);
			}

			mementoValueProducer.Ignore = true;
			return this;
		}

		/// <summary>
		///     Removes the Ignore of mementos on a certain property
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public MementoController Whitelist(string propertyName)
		{
			if (!MementoHost.MementoDataStore.TryGetValue(propertyName, out var mementoValueProducer))
			{
				mementoValueProducer = MementoHost.GetOrAddMemento(propertyName);
			}

			mementoValueProducer.Ignore = false;
			return this;
		}

		/// <summary>
		///     Goes forth or back in History. If
		///     <para>ages</para>
		///     is negative it goes back. if positive it goes forth
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="ages"></param>
		/// <returns></returns>
		public MementoController GoInHistory(string propertyName, int ages)
		{
			if (MementoHost.MementoDataStore.TryGetValue(propertyName, out var mementoValueProducer))
			{
				mementoValueProducer.GoInHistory(MementoHost, ages);
			}

			return this;
		}

		/// <summary>
		///     Goes forth or back in History. If
		///     <para>ages</para>
		///     is negative it goes back. if positive it goes forth
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="ages"></param>
		/// <returns></returns>
		public bool CanGoInHistory(string propertyName, int ages)
		{
			if (MementoHost.MementoDataStore.TryGetValue(propertyName, out var mementoValueProducer))
			{
				return mementoValueProducer.CanGoInHistory(ages);
			}

			return false;
		}

		public MementoController ImportSnapshot(MementoObjectSnapshot snapshot, ImportFlags flags = ImportFlags.Prefix)
		{
			lock (MementoHost.Lock)
			{
				foreach (var mementoPropertySnaptshot in snapshot.MementoSnapshots)
				{
					ImportSnapshot(mementoPropertySnaptshot, flags);
				}

				return this;
			}
		}

		public MementoController ImportSnapshot(MementoPropertySnaptshot snapshot, ImportFlags flags = ImportFlags.Prefix)
		{
			lock (MementoHost.Lock)
			{
				var memento = MementoHost.GetOrAddMemento(snapshot.PropertyName);
				switch (flags)
				{
					case ImportFlags.Append:
						memento.MementoData.PushRange(snapshot.MementoData.ToArray());

						break;
					case ImportFlags.Prefix:

						var peek = new ConcurrentStack<IMementoDataStamp>();
						peek.PushRange(snapshot.MementoData.ToArray());
						peek.PushRange(memento.MementoData.ToArray());
						memento.MementoData = peek;

						break;
				}

				return this;
			}
		}

		/// <summary>
		///     Returns a single Serializable Snapshot of the Current MementoData for a Property
		/// </summary>
		/// <returns></returns>
		public MementoController Snapshot(string propertyName, out MementoPropertySnaptshot snapshot)
		{
			MementoHost.MementoDataStore.TryGetValue(propertyName, out var mementoValueProducer);
			snapshot = mementoValueProducer?.CreateSnapshot();
			return this;
		}

		/// <summary>
		///     Returns a Serializable Snapshot of the Current MementoData
		/// </summary>
		/// <returns></returns>
		public MementoController Snapshot(out MementoObjectSnapshot snapshot)
		{
			snapshot = new MementoObjectSnapshot();
			snapshot.MementoSnapshots = new List<MementoPropertySnaptshot>();

			foreach (var mementoValueProducer in MementoHost.MementoData)
			{
				snapshot.MementoSnapshots.Add(mementoValueProducer.Value.CreateSnapshot());
			}

			return this;
		}

		/// <summary>
		///     Forgets all Memento data stored for the given
		///     <para>propertyName</para>
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public MementoController Forget(string propertyName, out MementoValueProducer data)
		{
			MementoHost.MementoDataStore.TryRemove(propertyName, out data);
			data.Forget();
			return this;
		}

		/// <summary>
		///     Forgets all Memento data stored for the given
		///     <para>propertyName</para>
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public MementoController Forget(string propertyName)
		{
			MementoValueProducer data;
			return Forget(propertyName, out data);
		}

		/// <summary>
		///     Forgets all Memento data
		/// </summary>
		/// <returns></returns>
		public MementoController Forget()
		{
			MementoHost.MementoDataStore.Clear();
			return this;
		}
	}
}