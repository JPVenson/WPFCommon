#region

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
// ReSharper disable ExplicitCallerInfoArgument

#endregion

namespace JPB.WPFBase.MVVM.ViewModel
{
#if !WINDOWS_UWP
	[Serializable]
	[DebuggerDisplay("TSOC - Count = {" + nameof(Count) + "}")]
#endif
	public class ThreadSaveObservableCollection<T> :
			AsyncViewModelBase,
			IReadOnlyList<T>,
			IProducerConsumerCollection<T>,
			IList,
			IList<T>,
			INotifyCollectionChanged,
#if !WINDOWS_UWP
			ICloneable,
#endif
			IDisposable
	{
		private ThreadSaveObservableCollection(IList<T> collection,bool copy)
			: this(DispatcherLock.GetDispatcher())
		{
			if (collection == null)
			{
				throw new ArgumentNullException("collection");
			}
			if (copy)
			{
				CopyFrom(collection);
			}
			else
			{
				_base = collection;
			}

			_actorHelper = this;
		}

		public ThreadSaveObservableCollection(IList<T> collection)
			: this(collection, true)
		{
		}

		public ThreadSaveObservableCollection()
			: this(DispatcherLock.GetDispatcher())
		{
		}

		public ThreadSaveObservableCollection(Dispatcher fromThread)
		{
			_actorHelper = new ViewModelBase(fromThread);
			_base = new Collection<T>();
		}

		private readonly IList<T> _base;
#if !WINDOWS_UWP
		[NonSerialized]
#endif
		private readonly ThreadSaveViewModelActor _actorHelper;

#if !WINDOWS_UWP
		[NonSerialized]
#endif
		private bool _batchCommit;
		protected bool BatchCommit
		{
			get { return _batchCommit; }
			set { _batchCommit = value; }
		}

		public bool ThreadSaveEnumeration { get; set; } = true;

		public bool IsReadOnlyOptimistic { get; set; }

		public object Clone()
		{
			lock (Lock)
			{
				var newCollection = new ThreadSaveObservableCollection<T>(this);
				return newCollection;
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			if (ThreadSaveEnumeration)
			{
				IEnumerator<T> enumerator = null;
				ThreadSaveAction(() => { enumerator = ToArray().Cast<T>().GetEnumerator(); });
				return enumerator;
			}
			return _base.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(T item)
		{
			Add(item as object);
		}

		public void Clear()
		{
			if (!CheckThrowReadOnlyException())
			{
				return;
			}
			_actorHelper.ThreadSaveAction(
			() =>
			{
				_base.Clear();
				SendPropertyChanged("Count");
				SendPropertyChanged("Item[]");
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			});
		}

		public bool Contains(T item)
		{
			return _base.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_base.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item)
		{
			if (!CheckThrowReadOnlyException())
			{
				return false;
			}
			var item2 = item;
			var result = false;
			_actorHelper.ThreadSaveAction(
			() =>
			{
				var index = IndexOf(item2);
				result = _base.Remove(item2);
				if (result)
				{
					SendPropertyChanged("Count");
					SendPropertyChanged("Item[]");
					OnCollectionChanged(
					new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item,
					index));
				}
			});
			return result;
		}

		public int Count => _base.Count;

		public bool IsReadOnly { get; set; }

		public void Dispose()
		{
			lock (Lock)
			{
				_base.Clear();
			}
		}

		public int Add(object value)
		{
			CheckType(value);
			if (!CheckThrowReadOnlyException())
			{
				return 0;
			}

			var tempitem = (T) value;
			var indexOf = -1;
			_actorHelper.ThreadSaveAction(
			() =>
			{
				indexOf = ((IList) _base).Add(tempitem);
				SendPropertyChanged("Count");
				SendPropertyChanged("Item[]");
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
				tempitem));
			});
			return indexOf;
		}

		public bool Contains(object value)
		{
			return ((IList) _base).Contains(value);
		}

		public int IndexOf(object value)
		{
			return ((IList) _base).IndexOf(value);
		}

		public void Insert(int index, object value)
		{
			CheckType(value);
			Insert(index, (T) value);
		}

		public void Remove(object value)
		{
			CheckType(value);
			Remove((T) value);
		}

		public bool IsFixedSize => IsReadOnly;

		public void RemoveAt(int index)
		{
			if (!CheckThrowReadOnlyException())
			{
				return;
			}
			_actorHelper.ThreadSaveAction(
			() =>
			{
				var old = _base[index];
				_base.RemoveAt(index);
				SendPropertyChanged("Count");
				SendPropertyChanged("Item[]");
				OnCollectionChanged(
				new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, old, index));
			});
		}

		object IList.this[int index]
		{
			get { return _base[index]; }
			set
			{
				if (!CheckThrowReadOnlyException())
				{
					return;
				}
				_base[index] = (T) value;
			}
		}

		public int IndexOf(T item)
		{
			return _base.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			if (!CheckThrowReadOnlyException())
			{
				return;
			}

			var tempitem = item;
			_actorHelper.ThreadSaveAction(
			() =>
			{
				_base.Insert(index, tempitem);
				SendPropertyChanged("Count");
				SendPropertyChanged("Item[]");
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
				tempitem, index));
			});
		}

		T IList<T>.this[int index]
		{
			get { return _base[index]; }
			set
			{
				if (!CheckThrowReadOnlyException())
				{
					return;
				}
				_base[index] = value;
			}
		}

		#region INotifyCollectionChanged Members

#if !WINDOWS_UWP
		[field: NonSerialized]
#endif
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		#endregion

		public void CopyTo(Array array, int index)
		{
			((ICollection) _base).CopyTo(array, index);
		}

		public object SyncRoot => Lock;

		public bool IsSynchronized => Monitor.IsEntered(Lock);

		public bool TryAdd(T item)
		{
			if (Monitor.IsEntered(Lock))
			{
				return false;
			}
			Add(item);
			return true;
		}

		public bool TryTake(out T item)
		{
			item = default(T);
			if (!Monitor.IsEntered(Lock))
			{
				lock (Lock)
				{
					item = this[Count - 1];
					return true;
				}
			}
			return false;
		}

		public T[] ToArray()
		{
			lock (Lock)
			{
				return _base.ToArray();
			}
		}

		public T this[int index]
		{
			get { return _base[index]; }
			set { SetItem(index, value); }
		}

		protected virtual void StartBatchCommit()
		{
			BatchCommit = true;
		}

		protected virtual void EndBatchCommit()
		{
			BatchCommit = false;
		}

		/// <summary>
		///     Batches commands into a single statement that will run when the delegate will retun true. Lock is optional but
		///     recommand
		/// </summary>
		/// <param name="action">
		///     You can Query against this collection. Its a copy and only collection actions as Add, Remove or
		///     else will be in Transaction
		/// </param>
		/// <param name="withLock">When True the Source collection will be locked as long as the Transaction is running</param>
		public void InTransaction(Func<ThreadSaveObservableCollection<T>, bool> action,
			bool withLock = true)
		{
			var cpy = Clone() as ThreadSaveObservableCollection<T>;
			if (cpy == null)
			{
				return;
			}
			try
			{
				if (withLock)
				{
					Monitor.Enter(Lock);
				}

				var events = new List<NotifyCollectionChangedEventArgs>();
				cpy.StartBatchCommit();
				cpy.CollectionChanged += (e, f) => { events.Add(f); };
				bool commit;
				try
				{
					commit = action(cpy);
				}
				catch
				{
					commit = false;
				}
				cpy.EndBatchCommit();
				if (commit)
				{
					foreach (var item in events)
					{
						switch (item.Action)
						{
							case NotifyCollectionChangedAction.Add:
								AddRange(item.NewItems.Cast<T>());
								break;
							case NotifyCollectionChangedAction.Remove:
								foreach (T innerItem in item.NewItems)
								{
									Remove(innerItem);
								}
								break;
							case NotifyCollectionChangedAction.Replace:
								SetItem(item.NewStartingIndex, (T) item.NewItems[0]);
								break;
							case NotifyCollectionChangedAction.Move:

								break;
							case NotifyCollectionChangedAction.Reset:
								Clear();
								break;
						}
					}
				}
			}
			finally
			{
				if (withLock)
				{
					Monitor.Exit(Lock);
				}
				cpy.Dispose();
			}
		}

		public static ThreadSaveObservableCollection<T> Wrap(ObservableCollection<T> batchServers)
		{
			return new ThreadSaveObservableCollection<T>(batchServers, true);
		}

		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			var handler = CollectionChanged;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		private void CopyFrom(IEnumerable<T> collection)
		{
			lock (Lock)
			{
				IList<T> items = _base;
				if (collection != null && items != null)
				{
					using (var enumerator = collection.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							items.Add(enumerator.Current);
						}
					}
				}
			}
		}

		public void AddRange(IEnumerable<T> item)
		{
			if (!CheckThrowReadOnlyException())
			{
				return;
			}
			var tempitem = item;
			var enumerable = tempitem as T[] ?? tempitem.ToArray();

			if (enumerable.Any())
			{
				_actorHelper.ThreadSaveAction(
				() =>
				{
					foreach (var variable in enumerable)
					{
						_base.Add(variable);
						SendPropertyChanged("Count");
						SendPropertyChanged("Item[]");
					}
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
					enumerable));
				});
			}
		}

		// ReSharper disable once UnusedParameter.Local
		private void CheckType(object value)
		{
			if (value != null && !(value is T))
			{
				throw new InvalidOperationException("object is not type of " + typeof(T));
			}
		}

		private bool CheckThrowReadOnlyException()
		{
			if (IsReadOnlyOptimistic)
			{
				return false;
			}

			if (IsReadOnly)
			{
				throw new NotSupportedException("This Collection was set to ReadOnly");
			}
			return true;
		}

		public void SetItem(int index, T newItem)
		{
			if (!CheckThrowReadOnlyException())
			{
				return;
			}

			T oldItem;
			_actorHelper.ThreadSaveAction(
			() =>
			{
				if (index + 1 > Count)
				{
					return;
				}

				oldItem = _base[index];
				_base.RemoveAt(index);
				_base.Insert(index, newItem);
				SendPropertyChanged("Count");
				SendPropertyChanged("Item[]");
				OnCollectionChanged(
				new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
				oldItem, newItem, index));
			});
		}
	}
}