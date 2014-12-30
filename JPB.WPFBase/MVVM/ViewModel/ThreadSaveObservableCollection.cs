using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace JPB.WPFBase.MVVM.ViewModel
{
    [Serializable, DebuggerDisplay("Count = {Count}"), ComVisible(false)]
    public class ThreadSaveObservableCollection<T> :
        Collection<T>,
        INotifyCollectionChanged,
        INotifyPropertyChanged
    {
        private readonly object LockObject = new object();
        private readonly ThreadSaveViewModelActor actorHelper;

        private ThreadSaveObservableCollection(IEnumerable<T> collection, bool copy)
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

        public ThreadSaveObservableCollection(List<T> list)
            : base((list != null) ? new List<T>(list.Count) : list)
        {
            CopyFrom(list);
            actorHelper = new ThreadSaveViewModelBase();
        }

        public ThreadSaveObservableCollection()
        {
            actorHelper = new ThreadSaveViewModelBase();
        }

        public ThreadSaveObservableCollection(Dispatcher fromThread)
        {
            actorHelper = new ThreadSaveViewModelBase(fromThread);
        }

        #region INotifyCollectionChanged Members

        [field: NonSerialized]
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public static ThreadSaveObservableCollection<T> Wrap<T>(ObservableCollection<T> batchServers)
        {
            return new ThreadSaveObservableCollection<T>(batchServers, true);
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (handler != null)
                handler(this, e);
        }

        protected override void InsertItem(int index, T item)
        {
            T tempitem = item;
            lock (LockObject)
            {
                base.InsertItem(index, tempitem);
                SendPropertyChanged("Count");
                SendPropertyChanged("Item[]");
                actorHelper.ThreadSaveAction(
                    () => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, tempitem, index)));
            }
        }

        public void AddRange(IEnumerable<T> item)
        {
            IEnumerable<T> tempitem = item;
            T[] enumerable = tempitem as T[] ?? tempitem.ToArray();
            foreach (T variable in enumerable)
                base.Add(variable);
            if (enumerable.Any())
            {
                actorHelper.ThreadSaveAction(
                    () =>
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, enumerable.Last())));
            }
        }

        protected override void ClearItems()
        {
            lock (LockObject)
            {
                base.ClearItems();
                actorHelper.ThreadSaveAction(
                    () =>
                    {
                        SendPropertyChanged("Count");
                        SendPropertyChanged("Item[]");
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    });
            }
        }

        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            T item;
            lock (LockObject)
            {
                if (oldIndex + 1 > this.Count)
                    return;
                item = base[oldIndex];
                base.RemoveItem(oldIndex);
                base.InsertItem(newIndex, item);
                actorHelper.ThreadSaveAction(
                    () =>
                    {
                        SendPropertyChanged("Item[]");
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Move, item,
                            newIndex, oldIndex));
                    });
            }

        }

        private void CopyFrom(IEnumerable<T> collection, bool copyRef = false)
        {
            lock (LockObject)
            {
                if (copyRef)
                {
                }
                else
                {
                    IList<T> items = base.Items;
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

        protected override void RemoveItem(int index)
        {
            T item;
            lock (LockObject)
            {
                if (index + 1 > this.Count)
                    return;

                item = base[index];
                base.RemoveItem(index);

                actorHelper.ThreadSaveAction(
                    () =>
                    {
                        SendPropertyChanged("Count");
                        SendPropertyChanged("Item[]");
                        OnCollectionChanged(
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item,
                                index));
                    });
            }
        }

        protected override void SetItem(int index, T item)
        {
            T oldItem;
            lock (LockObject)
            {
                //count is not Null based
                if (index + 1 > base.Count)
                    return;

                oldItem = base[index];
                base.SetItem(index, item);

                actorHelper.ThreadSaveAction(
                    () =>
                    {
                        SendPropertyChanged("Item[]");
                        OnCollectionChanged(
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                                oldItem, item, index));
                    });
            }

        }

        #region INotifyPropertyChanged

        /// <summary>
        ///     Raised when a property on this object has a new value
        /// </summary>
        /// <summary>
        ///     Raises this ViewModels PropertyChanged event
        /// </summary>
        /// <param name="propertyName">Name of the property that has a new value</param>
        public void SendPropertyChanged(string propertyName)
        {
            SendPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Raises this ViewModels PropertyChanged event
        /// </summary>
        /// <param name="e">Arguments detailing the change</param>
        protected virtual void SendPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        public void SendPropertyChanged<TProperty>(Expression<Func<TProperty>> property)
        {
            var lambda = (LambdaExpression)property;

            MemberExpression memberExpression;
            var body = lambda.Body as UnaryExpression;

            if (body != null)
            {
                UnaryExpression unaryExpression = body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else
                memberExpression = (MemberExpression)lambda.Body;
            SendPropertyChanged(memberExpression.Member.Name);
        }

        #endregion
    }
}