#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 13:05

#endregion

using System;
using System.Collections.Generic;

namespace WPFBase.MVVM.ViewModel
{
    public class ThreadSaveCollectionModel<T, E> : ThreadSaveViewModelBase
        where T : IEnumerable<E>, new()
    {
        public ThreadSaveCollectionModel()
        {
            Collection = new T();
            CreateFromSingelItem = s => new[] {s};
        }

        public ThreadSaveCollectionModel(Action sendPropChanged)
            : this()
        {
            SendPropChanged = sendPropChanged;
        }

        public ThreadSaveCollectionModel(Action sendPropChanged, Func<E, IEnumerable<E>> createFromSingelItem)
            : this(sendPropChanged)
        {
            CreateFromSingelItem = createFromSingelItem;
        }

        public ThreadSaveCollectionModel(Func<E, IEnumerable<E>> createFromSingelItem)
            : this(null, createFromSingelItem)
        {
        }

        public Action SendPropChanged { get; set; }
        public Func<E, IEnumerable<E>> CreateFromSingelItem { get; set; }

        #region Collection property

        private T _collection;

        public T Collection
        {
            get { return _collection; }
            set
            {
                _collection = value;
                SendCollectionChanged();
            }
        }

        #endregion

        #region SelectedItems property

        private IEnumerable<E> _selectedItems = default(IEnumerable<E>);

        public IEnumerable<E> SelectedItems
        {
            get { return _selectedItems; }
            set
            {
                _selectedItems = value;
                SendSelectedItemsChanged();
            }
        }

        #endregion

        #region SelectedItem property

        private E _selectedItem;

        public E SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                SelectedItems = CreateFromSingelItem(value);
                SendSelectedItemChanged();
            }
        }

        #endregion

        public void SendCollectionChanged()
        {
            base.BeginThreadSaveAction(() => SendPropertyChanged(() => Collection));
        }

        public void SendSelectedItemsChanged()
        {
            base.BeginThreadSaveAction(() => SendPropertyChanged(() => SelectedItems));
            if (SendPropChanged != null)
                base.BeginThreadSaveAction(SendPropChanged);
        }

        public void SendSelectedItemChanged()
        {
            base.BeginThreadSaveAction(() => SendPropertyChanged(() => SelectedItem));
            if (SendPropChanged != null)
                base.BeginThreadSaveAction(SendPropChanged);
        }
    }
}