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
    public class ThreadSaveObservableCollection<T> : Collection<T>, INotifyCollectionChanged, IList<T>, IList, INotifyPropertyChanged
    {
        public ThreadSaveObservableCollection(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this.CopyFrom(collection);
            actorHelper = new ThreadSaveViewModelBase();
        }

        public ThreadSaveObservableCollection(List<T> list)
            : base((list != null) ? new List<T>(list.Count) : list)
        {
            this.CopyFrom(list);
            actorHelper = new ThreadSaveViewModelBase();
        }

        public ThreadSaveObservableCollection()
        {
            actorHelper = new ThreadSaveViewModelBase();
        }

        [field: NonSerialized]
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        private ThreadSaveViewModelActor actorHelper;
        public event PropertyChangedEventHandler PropertyChanged;
        private object LockObject = new object();

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

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChanged;
            if (handler != null)
                handler(this, e);
        }

        protected override void InsertItem(int index, T item)
        {
            var tempitem = item;
            lock (LockObject)
            {

            }
            base.InsertItem(index, tempitem);
            this.SendPropertyChanged("Count");
            this.SendPropertyChanged("Item[]");
            actorHelper.ThreadSaveAction(() => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, tempitem)));
        }

        public void AddRange(IEnumerable<T> item)
        {
            var tempitem = item;
            var enumerable = tempitem as T[] ?? tempitem.ToArray();
            foreach (var variable in enumerable)
            {
                base.Add(variable);
            }
            if (enumerable.Any())
            actorHelper.ThreadSaveAction(() => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, enumerable.Last())));
        }

        protected override void ClearItems()
        {
            lock (LockObject)
            {
                base.ClearItems();
            }
           
            this.SendPropertyChanged("Count");
            this.SendPropertyChanged("Item[]");
            actorHelper.ThreadSaveAction(() => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)));
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
            this.SendPropertyChanged("Item[]");
            actorHelper.ThreadSaveAction(() => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex)));
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
                        {
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
                item = base[index];
                base.RemoveItem(index);
            }
            this.SendPropertyChanged("Count");
            this.SendPropertyChanged("Item[]");
            actorHelper.ThreadSaveAction(() => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, index)));
        }

        protected override void SetItem(int index, T item)
        {
            T oldItem;
            lock (LockObject)
            {
                oldItem = base[index];
                base.SetItem(index, item);
            }
            this.SendPropertyChanged("Item[]");
            actorHelper.ThreadSaveAction(() => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, oldItem, item, index)));
        }
    }
}