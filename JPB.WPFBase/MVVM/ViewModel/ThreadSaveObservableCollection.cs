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

namespace JPB.WPFBase.MVVM.ViewModel
{

#if !WINDOWS_UWP
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
#endif

    public class ThreadSaveObservableCollection<T> :
        AsyncViewModelBase,
        ICollection<T>,
        IEnumerable<T>,
        IReadOnlyList<T>,
        IReadOnlyCollection<T>,
        IProducerConsumerCollection<T>,
        IList,
        IList<T>,
        INotifyCollectionChanged,
#if !WINDOWS_UWP
        ICloneable,
#endif
        IDisposable
    {
        private readonly Collection<T> _base;
#if !WINDOWS_UWP
        [NonSerialized]
#endif
        private readonly ThreadSaveViewModelActor actorHelper;

#if !WINDOWS_UWP
        [NonSerialized]
#endif
        private bool _batchCommit;

        private ThreadSaveObservableCollection(IEnumerable<T> collection, bool copy)
            : this((Dispatcher)null)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            CopyFrom(collection);

            actorHelper = new ViewModelBase();
        }

        public ThreadSaveObservableCollection(IEnumerable<T> collection)
            : this(collection, false)
        {
        }

        public ThreadSaveObservableCollection()
            : this((Dispatcher)null)
        {
        }

        public ThreadSaveObservableCollection(Dispatcher fromThread)
        {
            actorHelper = new ViewModelBase(fromThread);
            _base = new Collection<T>();
        }

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
                return;
            lock (Lock)
            {
                actorHelper.ThreadSaveAction(
                    () =>
                    {
                        _base.Clear();
                        SendPropertyChanged("Count");
                        SendPropertyChanged("Item[]");
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    });
            }
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
                return false;
            T item2;
            lock (Lock)
            {
                item2 = item;
                var result = false;
                actorHelper.ThreadSaveAction(
                    () =>
                    {
                        var index = IndexOf(item2);
                        result = _base.Remove(item2);
                        SendPropertyChanged("Count");
                        SendPropertyChanged("Item[]");
                        OnCollectionChanged(
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item,
                                index));
                    });
                return result;
            }
        }

        public int Count
        {
            get { return _base.Count; }
        }

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
                return 0;

            lock (Lock)
            {
                var tempitem = (T)value;
                var indexOf = -1;
                actorHelper.ThreadSaveAction(
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
        }

        public bool Contains(object value)
        {
            return ((IList)_base).Contains(value);
        }

        public int IndexOf(object value)
        {
            return ((IList)_base).IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            CheckType(value);
            Insert(index, (T)value);
        }

        public void Remove(object value)
        {
            CheckType(value);
            Remove((T)value);
        }

        public bool IsFixedSize
        {
            get { return IsReadOnly; }
        }

        public void RemoveAt(int index)
        {
            if (!CheckThrowReadOnlyException())
                return;
            lock (Lock)
            {
                actorHelper.ThreadSaveAction(
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
        }

        object IList.this[int index]
        {
            get { return _base[index]; }
            set
            {
                if (!CheckThrowReadOnlyException())
                    return;
                _base[index] = (T)value;
            }
        }

        public int IndexOf(T item)
        {
            return _base.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (!CheckThrowReadOnlyException())
                return;

            var tempitem = item;
            lock (Lock)
            {
                _base.Insert(index, tempitem);
                SendPropertyChanged("Count");
                SendPropertyChanged("Item[]");
                actorHelper.ThreadSaveAction(
                    () =>
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                            tempitem, index)));
            }
        }

        T IList<T>.this[int index]
        {
            get { return _base[index]; }
            set
            {
                if (!CheckThrowReadOnlyException())
                    return;
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
            ((ICollection)_base).CopyTo(array, index);
        }

        public object SyncRoot
        {
            get { return Lock; }
        }

        public bool IsSynchronized { get; private set; }

        public bool TryAdd(T item)
        {
            if (Monitor.IsEntered(Lock))
                return false;
            Add(item);
            return true;
        }

        public bool TryTake(out T item)
        {
            item = default(T);
            if (!Monitor.IsEntered(Lock))
                lock (Lock)
                {
                    item = this[Count - 1];
                    return true;
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
        }


        protected virtual void StartBatchCommit()
        {
            _batchCommit = true;
        }

        protected virtual void EndBatchCommit()
        {
            _batchCommit = false;
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
            try
            {
                if (withLock)
                    Monitor.Enter(Lock);

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
                    foreach (var item in events)
                        switch (item.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                                AddRange(item.NewItems.Cast<T>());
                                break;
                            case NotifyCollectionChangedAction.Remove:
                                foreach (T innerItem in item.NewItems)
                                    Remove(innerItem);
                                break;
                            case NotifyCollectionChangedAction.Replace:
                                SetItem(item.NewStartingIndex, (T)item.NewItems[0]);
                                break;
                            case NotifyCollectionChangedAction.Move:

                                break;
                            case NotifyCollectionChangedAction.Reset:
                                Clear();
                                break;
                            default:
                                break;
                        }
            }
            finally
            {
                if (withLock)
                    Monitor.Exit(Lock);
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
                handler(this, e);
        }

        private void CopyFrom(IEnumerable<T> collection)
        {
            lock (Lock)
            {
                IList<T> items = _base;
                if (collection != null && items != null)
                    using (var enumerator = collection.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                            items.Add(enumerator.Current);
                    }
            }
        }

        public void AddRange(IEnumerable<T> item)
        {
            if (!CheckThrowReadOnlyException())
                return;
            lock (Lock)
            {
                var tempitem = item;
                var enumerable = tempitem as T[] ?? tempitem.ToArray();

                if (enumerable.Any())
                    actorHelper.ThreadSaveAction(
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

        private void CheckType(object value)
        {
            if (!(value is T))
                throw new InvalidOperationException("object is not type of " + typeof(T));
        }

        private bool CheckThrowReadOnlyException()
        {
            if (IsReadOnlyOptimistic)
                return false;

            if (IsReadOnly)
                throw new NotSupportedException("This Collection was set to ReadOnly");
            return true;
        }

        public void SetItem(int index, T newItem)
        {
            if (!CheckThrowReadOnlyException())
                return;

            T oldItem;
            lock (Lock)
            {
                //count is not 0 based
                if (index + 1 > Count)
                    return;

                actorHelper.ThreadSaveAction(
                    () =>
                    {
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
}