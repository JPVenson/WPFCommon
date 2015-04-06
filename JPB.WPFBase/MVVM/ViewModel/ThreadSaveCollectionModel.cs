#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 13:05

#endregion

using System;
using System.Collections;
using System.Collections.Generic;

namespace JPB.WPFBase.MVVM.ViewModel
{
    public class ThreadSaveCollectionModel<T, TE> : ThreadSaveViewModelBase, IEnumerable<TE>
        where T : IEnumerable<TE>, new()
    {
        public ThreadSaveCollectionModel()
        {
            Collection = new T();
            CreateFromSingelItem = s => new[] { s };
        }

        public ThreadSaveCollectionModel(Action sendPropChanged)
            : this()
        {
            SendPropChanged = sendPropChanged;
        }

        public ThreadSaveCollectionModel(Action sendPropChanged, Func<TE, IEnumerable<TE>> createFromSingelItem)
            : this(sendPropChanged)
        {
            CreateFromSingelItem = createFromSingelItem;
        }

        public ThreadSaveCollectionModel(Func<TE, IEnumerable<TE>> createFromSingelItem)
            : this(null, createFromSingelItem)
        {
        }

        public Action SendPropChanged { get; set; }
        public Func<TE, IEnumerable<TE>> CreateFromSingelItem { get; set; }

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

        private IEnumerable<TE> _selectedItems = default(IEnumerable<TE>);

        public IEnumerable<TE> SelectedItems
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

        private TE _selectedItem;

        public TE SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                SelectedItems = CreateFromSingelItem(value);
                SendSelectedItemChanged();
            }
        }

        #endregion

        public IEnumerator<TE> GetEnumerator()
        {
            return Collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

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

        //public static explicit operator ThreadSaveCollectionModel<T, TE>(T source)
        //{
        //    //return new ThreadSaveCollectionModel<ThreadSaveObservableCollection<TE>, TE>() { Collection = source };
        //    return new ThreadSaveCollectionModel<T, TE>() { Collection = source };
        //}

        public static implicit operator ThreadSaveCollectionModel<T, TE>(T source)
        {
            //return new ThreadSaveCollectionModel<ThreadSaveObservableCollection<TE>, TE>() { Collection = source };
            return new ThreadSaveCollectionModel<T, TE> { Collection = source };
        }
    }
}