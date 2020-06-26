#region

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Data;
using System.Windows.Threading;
using JetBrains.Annotations;

// ReSharper disable ExplicitCallerInfoArgument

#endregion

namespace JPB.WPFBase.MVVM.ViewModel
{
	/// <summary>
	///     Defines a collection that implements the <see cref="INotifyCollectionChanged" /> event in a dispatcher thread save
	///     manner.
	///     All Write Operations are synchronized to the Dispatcher. All Read operations will occur in the calling thread.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	[SuppressMessage("ReSharper", "NotResolvedInText")]
	[DebuggerTypeProxy(typeof(ThreadSaveObservableCollection<>.ThreadSaveObservableCollectionDebuggerProxy))]
	public class ThreadSaveObservableCollection<T> :
		ViewModelBase,
		IReadOnlyList<T>,
		IProducerConsumerCollection<T>,
		IList,
		IList<T>,
		INotifyCollectionChanged 
	{
		[NonSerialized]
		private readonly IList<T> _base;

		[NonSerialized]
		private bool _batchCommit;

		private ThreadSaveObservableCollection(IList<T> collection, bool copy)
			: this(DispatcherLock.GetDispatcher())
		{
			if (collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			if (copy)
			{
				CopyFrom(collection);
			}
			else
			{
				_base = collection;
			}
		}

		/// <inheritdoc />
		public ThreadSaveObservableCollection(IList<T> collection)
			: this(collection, true)
		{
		}

		/// <inheritdoc />
		public ThreadSaveObservableCollection()
			: this(DispatcherLock.GetDispatcher())
		{
		}

		/// <inheritdoc />
		public ThreadSaveObservableCollection(Dispatcher fromThread) : base(fromThread)
		{
			_base = new Collection<T>();
			CollectionViews = new ConcurrentDictionary<string, ICollectionView>();
		}

		/// <summary>
		///		Returns the Underlying Collection for Unlocked Access
		/// </summary>
		/// <returns></returns>
		public IList<T> GetUnderlyingCollection()
		{
			return _base;
		}

		/// <summary>
		///     Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		///     A new object that is a copy of this instance.
		/// </returns>
		public ThreadSaveObservableCollection<T> Clone()
		{
			return new ThreadSaveObservableCollection<T>(this);
		}

		/// <inheritdoc cref="ICollection" />
		public void Clear()
		{
			if (!CheckThrowReadOnlyException())
			{
				return;
			}

			ThreadSaveAction(
				() =>
				{
					_base.Clear();
					SendPropertyChanged(nameof(Count));
					SendPropertyChanged("Item[]");
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
				});
		}

		/// <inheritdoc cref="ICollection" />
		public bool IsReadOnly { get; set; }

		/// <inheritdoc />
		public int Add(object value)
		{
			CheckType(value);
			if (!CheckThrowReadOnlyException())
			{
				return 0;
			}

			var tempItem = (T) value;
			var indexOf = -1;
			ThreadSaveAction(
				() =>
				{
					indexOf = ((IList) _base).Add(tempItem);
					SendPropertyChanged(nameof(Count));
					SendPropertyChanged("Item[]");
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
						tempItem, indexOf));
				});
			return indexOf;
		}

		/// <inheritdoc />
		public bool Contains(object value)
		{
			return ((IList) _base).Contains(value);
		}

		/// <inheritdoc />
		public int IndexOf(object value)
		{
			return ((IList) _base).IndexOf(value);
		}

		/// <inheritdoc />
		public void Insert(int index, object value)
		{
			CheckType(value);
			Insert(index, (T) value);
		}

		/// <inheritdoc />
		public void Remove(object value)
		{
			CheckType(value);
			Remove((T) value);
		}

		/// <inheritdoc />
		public bool IsFixedSize
		{
			get { return IsReadOnly; }
		}

		/// <inheritdoc cref="ICollection" />
		public void RemoveAt(int index)
		{
			if (!CheckThrowReadOnlyException())
			{
				return;
			}

			ThreadSaveAction(
				() =>
				{
					var old = _base[index];
					_base.RemoveAt(index);
					SendPropertyChanged(nameof(Count));
					SendPropertyChanged("Item[]");
					OnCollectionChanged(
						new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, old, index));
				});
		}

		/// <inheritdoc />
		object IList.this[int index]
		{
			get { return _base[index]; }
			set
			{
				if (!CheckThrowReadOnlyException())
				{
					return;
				}
				ReplaceItem(index, (T) value);
				//_base[index] = (T) value;
			}
		}

		/// <inheritdoc />
		public void Add(T item)
		{
			Add(item as object);
		}

		/// <inheritdoc />
		public bool Contains(T item)
		{
			return _base.Contains(item);
		}

		/// <inheritdoc />
		public bool Remove(T item)
		{
			if (!CheckThrowReadOnlyException())
			{
				return false;
			}

			var result = false;
			ThreadSaveAction(
				() =>
				{
					var index = IndexOf(item);
					result = _base.Remove(item);
					if (result)
					{
						SendPropertyChanged(nameof(Count));
						SendPropertyChanged("Item[]");
						OnCollectionChanged(
							new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item,
								index));
					}
				});
			return result;
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

			ThreadSaveAction(
				() =>
				{
					_base.Insert(index, item);
					SendPropertyChanged(nameof(Count));
					SendPropertyChanged("Item[]");
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
						item, index));
				});
		}

		/// <inheritdoc />
		T IList<T>.this[int index]
		{
			get { return _base[index]; }
			set
			{
				if (!CheckThrowReadOnlyException())
				{
					return;
				}
				
				ReplaceItem(index, value);
				//_base[index] = value;
			}
		}

		#region INotifyCollectionChanged Members

#if !WINDOWS_UWP
		/// <inheritdoc />
		[field: NonSerialized]
#endif
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		#endregion

		/// <inheritdoc cref="ICollection" />
		public void CopyTo(T[] array, int arrayIndex)
		{
			_base.CopyTo(array, arrayIndex);
		}

		/// <inheritdoc />
		public void CopyTo(Array array, int index)
		{
			((ICollection) _base).CopyTo(array, index);
		}

		/// <inheritdoc />
		public object SyncRoot
		{
			get { return Dispatcher; }
		}

		/// <inheritdoc />
		public bool IsSynchronized
		{
			get { return Dispatcher.CheckAccess(); }
		}

		/// <inheritdoc />
		public bool TryAdd(T item)
		{
			if (!IsSynchronized)
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
			if (IsSynchronized)
			{
				item = this[Count - 1];
				return true;
			}

			return false;
		}

		/// <inheritdoc />
		public T[] ToArray()
		{
			if (ThreadSaveEnumeration)
			{
				T[] result = null;
				ThreadSaveAction(() =>
				{
					result = _base.ToArray();
				});
				return result;
			}

			return _base.ToArray();
		}

		/// <inheritdoc />
		[MustUseReturnValue]
		public IEnumerator<T> GetEnumerator()
		{
			if (ThreadSaveEnumeration)
			{
				IEnumerator<T> enumerator = null;
				ThreadSaveAction(() =>
				{
					enumerator = ((IEnumerable<T>)_base.ToArray()).GetEnumerator();
				});
				return enumerator;
			}

			return _base.GetEnumerator();
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <inheritdoc cref="ICollection" />
		public int Count
		{
			get { return _base.Count; }
		}

		/// <inheritdoc />
		public T this[int index]
		{
			get { return _base[index]; }
			set { ReplaceItem(index, value); }
		}

		/// <summary>
		///     Obtains a ether new Collection view or a cached one
		/// </summary>
		/// <returns></returns>
		[MustUseReturnValue]
		public ICollectionView ObtainView(string key = "Default")
		{
			if (CollectionViews.ContainsKey(key))
			{
				return CollectionViews[key];
			}

			ICollectionView source = null;
			ThreadSaveAction(() =>
			{
				if (key == "Default")
				{
					source = CollectionViews[key] = CollectionViewSource.GetDefaultView(this);
				}

				source = CollectionViews[key] = new ListCollectionView(this);
			});
			return source;
		}

		/// <summary>
		///     Starts the batch commit.
		/// </summary>
		protected virtual void StartBatchCommit()
		{
			BatchCommit = true;
		}

		/// <summary>
		///     Ends the batch commit.
		/// </summary>
		protected virtual void EndBatchCommit()
		{
			BatchCommit = false;
		}
		
		/// <summary>
		///     Batches commands into a single statement that will run when the delegate will return true. Lock is optional but
		///     recommend
		/// </summary>
		/// <param name="action">
		///     You can Query against this collection. Its a copy and only collection actions as Add, Remove or
		///     else will be in Transaction
		/// </param>
		/// <param name="withLock">
		///     When True the Source collection will be locked as long as the Transaction is running
		/// </param>
		[PublicAPI]
		public void InTransaction(Func<ThreadSaveObservableCollection<T>, bool> action,
			bool withLock = true)
		{
			if (!(Clone() is ThreadSaveObservableCollection<T> cpy))
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
				if (!commit)
				{
					return;
				}

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
							ReplaceItem(item.NewStartingIndex, (T) item.NewItems[0]);
							break;
						case NotifyCollectionChangedAction.Move:

							break;
						case NotifyCollectionChangedAction.Reset:
							Clear();
							break;
					}
				}
			}
			finally
			{
				if (withLock)
				{
					Monitor.Exit(Lock);
				}
			}
		}

		/// <summary>
		///     Raises the <see cref="INotifyCollectionChanged.CollectionChanged" /> event
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			CollectionChanged?.Invoke(this, e);
		}

		private void CopyFrom(IEnumerable<T> collection)
		{
			var items = _base;
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

		/// <summary>
		///     Adds all items in the IEnumerable in one batch.
		///     Some WPF Controls do not allow Ranges to be add. If an Error occurs call the <see cref="AddEach" /> method
		/// </summary>
		/// <param name="item">The item.</param>
		public void AddRange(IEnumerable<T> item)
		{
			if (!CheckThrowReadOnlyException())
			{
				return;
			}

			var enumerable = item as T[] ?? item.ToArray();

			if (enumerable.Any())
			{
				ThreadSaveAction(
					() =>
					{
						foreach (var variable in enumerable)
						{
							_base.Add(variable);
							SendPropertyChanged(nameof(Count));
							SendPropertyChanged("Item[]");
						}

						OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
							enumerable));
					});
			}
		}

		/// <summary>
		///     Adds all items in the IEnumerable individual.
		/// </summary>
		/// <param name="collection">The item.</param>
		public void AddEach(IEnumerable<T> collection)
		{
			if (!CheckThrowReadOnlyException())
			{
				return;
			}

			var enumerable = collection as T[] ?? collection.ToArray();

			if (enumerable.Any())
			{
				ThreadSaveAction(
					() =>
					{
						foreach (var variable in enumerable)
						{
							Add(variable);
						}
					});
			}
		}

		// ReSharper disable once UnusedParameter.Local
		private void CheckType([CanBeNull] object value)
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

		/// <summary>
		///     Inserts an Item on the given index
		/// </summary>
		/// <param name="index"></param>
		/// <param name="newItem"></param>
		public void ReplaceItem(int index, T newItem)
		{
			if (!CheckThrowReadOnlyException())
			{
				return;
			}

			T oldItem;
			ThreadSaveAction(
				() =>
				{
					if (index + 1 > Count)
					{
						return;
					}

					oldItem = _base[index];
					_base.RemoveAt(index);
					_base.Insert(index, newItem);
					SendPropertyChanged(nameof(Count));
					SendPropertyChanged("Item[]");
					OnCollectionChanged(
						new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
							oldItem, newItem, index));
				});
		}

	

		/// <summary>
		///     Gets or sets a value indicating whether [batch commit].
		/// </summary>
		/// <value>
		///     <c>true</c> if [batch commit]; otherwise, <c>false</c>.
		/// </value>
		protected bool BatchCommit
		{
			get { return _batchCommit; }
			set { _batchCommit = value; }
		}

		/// <summary>
		///     Gets or Sets a if an Enumeration of this Collection should occur in a ThreadSave manner
		/// </summary>
		public bool ThreadSaveEnumeration { get; set; } = true;

		/// <summary>
		///     Gets or sets a value indicating whether this instance is read only optimistic.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is read only optimistic; otherwise, <c>false</c>.
		/// </value>
		public bool IsReadOnlyOptimistic { get; set; }

		private IDictionary<string, ICollectionView> CollectionViews { get; }

		private class ThreadSaveObservableCollectionDebuggerProxy : IEnumerable<T>
		{
			private readonly ThreadSaveObservableCollection<T> _source;

			public ThreadSaveObservableCollectionDebuggerProxy(ThreadSaveObservableCollection<T> source)
			{
				_source = source;
			}

			public IEnumerator<T> GetEnumerator()
			{
				return _source._base.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable) _source).GetEnumerator();
			}

			public IEnumerable<T> Items
			{
				get { return _source._base; }
			}

			public int Count
			{
				get { return _source.Count; }
			}

			public bool IsReadOnly
			{
				get { return _source.IsReadOnly; }
			}

			public object SyncRoot
			{
				get { return _source.Lock; }
			}

			public bool IsSynchronized
			{
				get { return _source.IsSynchronized; }
			}

			/// <summary>
			///     Gets or Sets a if an Enumeration of this Collection should occur in a ThreadSave manner
			/// </summary>
			public bool ThreadSaveEnumeration
			{
				get { return _source.ThreadSaveEnumeration; }
			}

			/// <summary>
			///     Gets or sets a value indicating whether this instance is read only optimistic.
			/// </summary>
			/// <value>
			///     <c>true</c> if this instance is read only optimistic; otherwise, <c>false</c>.
			/// </value>
			public bool IsReadOnlyOptimistic
			{
				get { return _source.IsReadOnlyOptimistic; }
			}
		}
	}
}