#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 13:05

#endregion

using System;
using System.Collections;
using System.Collections.Generic;

namespace JPB.WPFBase.MVVM.ViewModel
{
    public class ThreadSaveCollectionModel<CollectionType, CollectionGenerica> : ThreadSaveViewModelBase, IEnumerable<CollectionGenerica>
        where CollectionType : IEnumerable<CollectionGenerica>, new()
    {
        public ThreadSaveCollectionModel()
        {
            this.Collection = new CollectionType();
            CreateFromSingelItem = s => new[] { s };
        }

        public ThreadSaveCollectionModel(Action sendPropChanged)
            : this()
        {
            SendPropChanged = sendPropChanged;
        }

        public ThreadSaveCollectionModel(Action sendPropChanged, Func<CollectionGenerica, IEnumerable<CollectionGenerica>> createFromSingelItem)
            : this(sendPropChanged)
        {
            CreateFromSingelItem = createFromSingelItem;
        }

        public ThreadSaveCollectionModel(Func<CollectionGenerica, IEnumerable<CollectionGenerica>> createFromSingelItem)
            : this(null, createFromSingelItem)
        {
        }

        public Action SendPropChanged { get; set; }
        public Func<CollectionGenerica, IEnumerable<CollectionGenerica>> CreateFromSingelItem { get; set; }

        #region Collection property

        private CollectionType _collection;

        public CollectionType Collection
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

        private IEnumerable<CollectionGenerica> _selectedItems = default(IEnumerable<CollectionGenerica>);

        public IEnumerable<CollectionGenerica> SelectedItems
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

        private CollectionGenerica _selectedItem;

        public CollectionGenerica SelectedItem
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

        public IEnumerator<CollectionGenerica> GetEnumerator()
        {
            return this.Collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void SendCollectionChanged()
        {
            base.BeginThreadSaveAction(() => SendPropertyChanged(() => this.Collection));
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

        public static implicit operator ThreadSaveCollectionModel<CollectionType, CollectionGenerica>(CollectionType source)
        {
            //return new ThreadSaveCollectionModel<ThreadSaveObservableCollection<TE>, TE>() { Collection = source };
            return new ThreadSaveCollectionModel<CollectionType, CollectionGenerica> { Collection = source };
        }
    }
}