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
using JetBrains.Annotations;

// ReSharper disable ExplicitCallerInfoArgument

#endregion

namespace JPB.WPFBase.MVVM.ViewModel
{
	/// <summary>
	/// Provides Access to a Collection in a Dispatcher-Thread Save manner
	/// </summary>
	/// <typeparam name="T"></typeparam>
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
		private ThreadSaveObservableCollection(IList<T> collection, bool copy)
			: this((Dispatcher)null)
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

		/// <inheritdoc />
		public ThreadSaveObservableCollection(IList<T> collection)
			: this(collection, true)
		{
		}

		/// <inheritdoc />
		public ThreadSaveObservableCollection()
			: this((Dispatcher)null)
		{
		}

		/// <inheritdoc />
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
		/// <summary>
		/// Is this Collection Currently in a Batch Operation that can be Reverted
		/// </summary>
		protected bool BatchCommit
		{
			get { return _batchCommit; }
			set { _batchCommit = value; }
		}

		/// <summary>
		/// Should the Enumeration of this Collection be Dispatcher-Thread Save
		/// </summary>
		public bool ThreadSaveEnumeration { get; set; } = true;

		/// <summary>
		/// If this Collection is ReadOnly should be an Exception be thrown.
		/// When <value>True</value> no Exception is thrown
		/// </summary>
		public bool IsReadOnlyOptimistic { get; set; }

		/// <summary>
		/// Create a Copy of this TSOC
		/// </summary>
		/// <returns></returns>
		public object Clone()
		{
			lock (Lock)
			{
				var newCollection = new ThreadSaveObservableCollection<T>(this);
				return newCollection;
			}
		}

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator()
		{
			if (!ThreadSaveEnumeration)
			{
				return _base.GetEnumerator();
			}
			IEnumerator<T> enumerator = null;
			// ReSharper disable once GenericEnumeratorNotDisposed
			ThreadSaveAction(() => { enumerator = ToArray().Cast<T>().GetEnumerator(); });
			return enumerator;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <inheritdoc />
		public void Add(T item)
		{
			Add(item as object);
		}

		/// <summary>
		/// Clears all items
		/// </summary>
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

		/// <inheritdoc />
		public bool Contains(T item)
		{
			return _base.Contains(item);
		}

		/// <summary>Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based indexing. </param>
		/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins. </param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="array" /> is null. </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="index" /> is less than zero. </exception>
		/// <exception cref="T:System.ArgumentException">
		/// <paramref name="array" /> is multidimensional.-or- The number of elements in the source <see cref="T:System.Collections.ICollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.-or-The type of the source <see cref="T:System.Collections.ICollection" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>

		public void CopyTo(T[] array, int index)
		{
			_base.CopyTo(array, index);
		}

		/// <inheritdoc />
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
				if (!result)
				{
					return;
				}
				SendPropertyChanged("Count");
				SendPropertyChanged("Item[]");
				OnCollectionChanged(
				new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item,
				index));
			});
			return result;
		}

		/// <inheritdoc cref="IReadOnlyList{T}" />
		public int Count => _base.Count;

		/// <inheritdoc />
		public bool IsReadOnly { get; set; }

		/// <inheritdoc />
		public void Dispose()
		{
			lock (Lock)
			{
				_base.Clear();
			}
		}

		/// <inheritdoc />
		public int Add(object value)
		{
			CheckType(value);
			if (!CheckThrowReadOnlyException())
			{
				return 0;
			}

			var tempitem = (T)value;
			var indexOf = -1;
			_actorHelper.ThreadSaveAction(
			() =>
			{
				indexOf = ((IList)_base).Add(tempitem);
				SendPropertyChanged("Count");
				SendPropertyChanged("Item[]");
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
				tempitem));
			});
			return indexOf;
		}

		/// <inheritdoc />
		public bool Contains(object value)
		{
			return ((IList)_base).Contains(value);
		}

		/// <inheritdoc />
		public int IndexOf(object value)
		{
			return ((IList)_base).IndexOf(value);
		}

		/// <inheritdoc />
		public void Insert(int index, object value)
		{
			CheckType(value);
			Insert(index, (T)value);
		}

		/// <inheritdoc />
		public void Remove(object value)
		{
			CheckType(value);
			Remove((T)value);
		}

		/// <inheritdoc />
		public bool IsFixedSize => IsReadOnly;

		/// <inheritdoc />
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
				_base[index] = (T)value;
			}
		}

		/// <inheritdoc />
		public int IndexOf(T item)
		{
			return _base.IndexOf(item);
		}

		/// <inheritdoc />
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

		/// <inheritdoc />
		public void CopyTo(Array array, int index)
		{
			((ICollection)_base).CopyTo(array, index);
		}

		/// <inheritdoc />
		public object SyncRoot => Lock;

		/// <inheritdoc />
		public bool IsSynchronized => Monitor.IsEntered(Lock);

		/// <inheritdoc />
		public bool TryAdd(T item)
		{
			if (Monitor.IsEntered(Lock))
			{
				return false;
			}
			Add(item);
			return true;
		}

		/// <inheritdoc />
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

		/// <inheritdoc />
		public T[] ToArray()
		{
			lock (Lock)
			{
				return ToArrayUnsafe();
			}
		}
		/// <summary>
		/// Returns a Copy of the current Collection as an Array without checking for Thread-Savety
		/// </summary>
		/// <returns></returns>
		public T[] ToArrayUnsafe()
		{
			return _base.ToArray();
		}

		/// <inheritdoc />
		public T this[int index]
		{
			get { return _base[index]; }
			set { SetItem(index, value); }
		}

		/// <summary>
		/// Indicates a Batch commiting start. Does
		/// </summary>
		protected virtual void StartBatchCommit()
		{
			BatchCommit = true;
		}

		/// <summary>
		/// Indicates a Batch commiting end
		/// </summary>
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
								SetItem(item.NewStartingIndex, (T)item.NewItems[0]);
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


		/// <summary>
		/// Event Handler for the <code>INotifyCollectionChanged</code> Event.
		/// Always ThreadSave
		/// </summary>
		/// <param name="e"></param>
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
		/// <summary>
		/// Does not use the AddRange event. Adds each item in an Atom-Like operation
		/// </summary>
		/// <param name="items"></param>
		public void AddEach(IEnumerable<T> items)
		{
			if (!CheckThrowReadOnlyException())
			{
				return;
			}
			var tempitem = items;
			var enumerable = tempitem as T[] ?? tempitem.ToArray();

			if (enumerable.Any())
			{
				_actorHelper.ThreadSaveAction(
				() =>
				{
					foreach (var item in enumerable)
					{
						Add(item);
					}
				});
			}
		}
		/// <summary>
		/// Adds all Items by using the AddRange event
		/// </summary>
		/// <param name="items"></param>
		public void AddRange(IEnumerable<T> items)
		{
			if (!CheckThrowReadOnlyException())
			{
				return;
			}
			var tempitem = items;
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

		[AssertionMethod]
		private static void CheckType(object value)
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

		/// <inheritdoc />
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