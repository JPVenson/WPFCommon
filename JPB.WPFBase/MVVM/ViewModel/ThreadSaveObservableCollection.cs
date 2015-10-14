using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Threading;

namespace JPB.WPFBase.MVVM.ViewModel
{
    [Serializable, DebuggerDisplay("Count = {Count}")]
    public class ThreadSaveObservableCollection<T> :
        AsyncViewModelBase,
        ICollection<T>,
        IEnumerable<T>,
        IReadOnlyList<T>,
        IReadOnlyCollection<T>,
        IProducerConsumerCollection<T>,
        IList,
        IList<T>,
        INotifyCollectionChanged
    {
        private readonly ThreadSaveViewModelActor actorHelper;

        private readonly Collection<T> _base;

        private ThreadSaveObservableCollection(IEnumerable<T> collection, bool copy)
            : this((Dispatcher)null)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            CopyFrom(collection);

            actorHelper = new ThreadSaveViewModelBase();
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
            actorHelper = new ThreadSaveViewModelBase(fromThread);
            _base = new Collection<T>();
        }

        #region INotifyCollectionChanged Members

        [field: NonSerialized]
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion
        public static ThreadSaveObservableCollection<T> Wrap(ObservableCollection<T> batchServers)
        {
            return new ThreadSaveObservableCollection<T>(batchServers, true);
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (handler != null)
                handler(this, e);
        }

        private void CopyFrom(IEnumerable<T> collection, bool copyRef = false)
        {
            lock (Lock)
            {
                if (copyRef)
                {
                }
                else
                {
                    IList<T> items = _base;
                    if ((collection != null) && (items != null))
                    {
                        using (IEnumerator<T> enumerator = collection.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                                items.Add(enumerator.Current);
                        }
                    }
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _base.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_base).GetEnumerator();
        }

        public void Add(T item)
        {
            this.Add(item as object);
        }

        public void AddRange(IEnumerable<T> item)
        {
            if (!this.CheckThrowReadOnlyException())
                return;
            lock (Lock)
            {
                IEnumerable<T> tempitem = item;
                T[] enumerable = tempitem as T[] ?? tempitem.ToArray();
                foreach (T variable in enumerable)
                {
                    _base.Add(variable);
                    SendPropertyChanged("Count");
                    SendPropertyChanged("Item[]");
                }
                if (enumerable.Any())
                {
                    actorHelper.ThreadSaveAction(
                        () =>
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, enumerable)));
                }
            }
        }

        private void CheckType(object value)
        {
            if (!(value is T))
                throw new InvalidOperationException("object is not type of T");
        }

        public int Add(object value)
        {
            CheckType(value);
            if (!this.CheckThrowReadOnlyException())
                return 0;

            lock (Lock)
            {
                var tempitem = (T)value;
                var indexOf = ((IList)_base).Add(tempitem);
                actorHelper.ThreadSaveAction(
                    () =>
                    {
                        SendPropertyChanged("Count");
                        SendPropertyChanged("Item[]");
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, tempitem));
                    });
                return indexOf;
            }
        }

        public bool Contains(object value)
        {
            return ((IList)_base).Contains(value);
        }

        public void Clear()
        {
            if (!this.CheckThrowReadOnlyException())
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

        public int IndexOf(object value)
        {
            return ((IList)_base).IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            CheckType(value);
            this.Insert(index, (T)value);
        }

        public void Remove(object value)
        {
            CheckType(value);
            this.Remove((T)value);
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
            if (!this.CheckThrowReadOnlyException())
                return false;
            T item2;
            lock (Lock)
            {
                item2 = item;
                var index = IndexOf(item2);
                var result = _base.Remove(item2);

                actorHelper.ThreadSaveAction(
                    () =>
                    {
                        SendPropertyChanged("Count");
                        SendPropertyChanged("Item[]");
                        OnCollectionChanged(
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item,
                                index));
                    });
                return result;
            }
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)_base).CopyTo(array, index);
        }

        public int Count
        {
            get { return _base.Count; }
        }

        public object SyncRoot { get { return Lock; } }

        public bool IsSynchronized { get; private set; }

        public bool IsReadOnly
        {
            get;
            set;
        }

        public bool IsReadOnlyOptimistic
        {
            get;
            set;
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public int IndexOf(T item)
        {
            lock (Lock)
            {
                return _base.IndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            if (!this.CheckThrowReadOnlyException())
                return;

            T tempitem = item;
            lock (Lock)
            {
                _base.Insert(index, tempitem);
                SendPropertyChanged("Count");
                SendPropertyChanged("Item[]");
                actorHelper.ThreadSaveAction(() => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, tempitem, index)));
            }
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
            if (!this.CheckThrowReadOnlyException())
                return;
            T oldItem;
            lock (Lock)
            {
                //count is not 0 based
                if (index + 1 > Count)
                    return;

                oldItem = _base[index];
                _base.RemoveAt(index);
                _base.Insert(index, newItem);

                actorHelper.ThreadSaveAction(
                    () =>
                    {
                        SendPropertyChanged("Count");
                        SendPropertyChanged("Item[]");
                        OnCollectionChanged(
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                                oldItem, newItem, index));
                    });
            }
        }

        public void RemoveAt(int index)
        {
            if (!this.CheckThrowReadOnlyException())
                return;
            lock (Lock)
            {
                var old = _base[index];
                _base.RemoveAt(index);
                actorHelper.ThreadSaveAction(
                 () =>
                 {
                     SendPropertyChanged("Count");
                     SendPropertyChanged("Item[]");
                     OnCollectionChanged(
                         new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, old, index));
                 });
            }
        }

        public bool TryAdd(T item)
        {
            if(Monitor.IsEntered(this.Lock))
            {
                return false;
            }
            Add(item);
            return true;
        }

        public bool TryTake(out T item)
        {
            item = default(T);
            if (!Monitor.IsEntered(this.Lock))
            {
                lock (this.Lock)
                {
                    item = this[Count - 1];
                    return true;
                }
            }
            return false;
        }

        public T[] ToArray()
        {
            lock (this.Lock)
            {
                return _base.ToArray();
            }
        }
        
        object IList.this[int index]
        {
            get { return _base[index]; }
            set
            {
                if (!this.CheckThrowReadOnlyException())
                    return;
                _base[index] = (T)value;
            }
        }

        T IList<T>.this[int index]
        {
            get { return _base[index]; }
            set
            {
                if (!this.CheckThrowReadOnlyException())
                    return;
                _base[index] = value;
            }
        }

        public T this[int index]
        {
            get { return _base[index]; }
        }
    }
}