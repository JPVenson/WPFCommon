using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace WPFBase.MVVM.ViewModel
{
    [Serializable, DebuggerDisplay("Count = {Count}"), ComVisible(false)]
    public class ThreadSaveObservableCollection<T> : Collection<T>, INotifyCollectionChanged, IList<T>, IList,
        INotifyPropertyChanged
    {
        private readonly object LockObject = new object();
        private readonly ThreadSaveViewModelActor actorHelper;

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
            var lambda = (LambdaExpression) property;

            MemberExpression memberExpression;
            var body = lambda.Body as UnaryExpression;

            if (body != null)
            {
                UnaryExpression unaryExpression = body;
                memberExpression = (MemberExpression) unaryExpression.Operand;
            }
            else
                memberExpression = (MemberExpression) lambda.Body;
            SendPropertyChanged(memberExpression.Member.Name);
        }

        #endregion

        public ThreadSaveObservableCollection(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            CopyFrom(collection);
            actorHelper = new ThreadSaveViewModelBase();
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

        #region INotifyCollectionChanged Members

        [field: NonSerialized]
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

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
            }
            base.InsertItem(index, tempitem);
            SendPropertyChanged("Count");
            SendPropertyChanged("Item[]");
            actorHelper.ThreadSaveAction(
                () =>
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, tempitem)));
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
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                            enumerable.Last())));
            }
        }

        protected override void ClearItems()
        {
            lock (LockObject)
            {
                base.ClearItems();
            }

            SendPropertyChanged("Count");
            SendPropertyChanged("Item[]");
            actorHelper.ThreadSaveAction(
                () => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)));
        }

        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            T item;
            lock (LockObject)
            {
                item = base[oldIndex];
                base.RemoveItem(oldIndex);
                base.InsertItem(newIndex, item);
            }
            SendPropertyChanged("Item[]");
            actorHelper.ThreadSaveAction(
                () =>
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item,
                        newIndex, oldIndex)));
        }

        private void CopyFrom(IEnumerable<T> collection)
        {
            lock (LockObject)
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

        protected override void RemoveItem(int index)
        {
            T item;
            lock (LockObject)
            {
                item = base[index];
                base.RemoveItem(index);
            }
            SendPropertyChanged("Count");
            SendPropertyChanged("Item[]");
            actorHelper.ThreadSaveAction(
                () =>
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item,
                        index)));
        }

        protected override void SetItem(int index, T item)
        {
            T oldItem;
            lock (LockObject)
            {
                oldItem = base[index];
                base.SetItem(index, item);
            }
            SendPropertyChanged("Item[]");
            actorHelper.ThreadSaveAction(
                () =>
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                        oldItem, item, index)));
        }
    }
}