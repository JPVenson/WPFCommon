#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 13:05

#endregion

using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

namespace JPB.WPFBase.MVVM.ViewModel
{
    public class ThreadSaveCollectionModel<T, TC> : ViewModelBase,
        IEnumerable<TC>
        where T : IEnumerable<TC>, new()
    {
        public ThreadSaveCollectionModel()
        {
            this.Collection = new T();
            FilterView = CollectionViewSource.GetDefaultView(this.Collection);
            if (CreateFromSingelItem != null)
                CreateFromSingelItem = s => new[] { s };
        }

        public ThreadSaveCollectionModel(Action sendPropChanged)
            : this()
        {
            if (sendPropChanged == null) throw new ArgumentNullException(nameof(sendPropChanged));
            SendPropChanged = sendPropChanged;
        }

        public ThreadSaveCollectionModel(Action sendPropChanged, Func<TC, IEnumerable<TC>> createFromSingelItem)
            : this(sendPropChanged)
        {
            if (createFromSingelItem == null) throw new ArgumentNullException(nameof(createFromSingelItem));
            CreateFromSingelItem = createFromSingelItem;
        }

        public ThreadSaveCollectionModel(Func<TC, IEnumerable<TC>> createFromSingelItem)
            : this(null, createFromSingelItem)
        {
        }

        public ICollectionView FilterView { get; set; }
        public Action SendPropChanged { get; set; }
        public Func<TC, IEnumerable<TC>> CreateFromSingelItem { get; set; }

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

        private IEnumerable<TC> _selectedItems = default(IEnumerable<TC>);

        public IEnumerable<TC> SelectedItems
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

        private TC _selectedItem;

        public TC SelectedItem
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

        public IEnumerator<TC> GetEnumerator()
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

        //public static implicit operator ThreadSaveCollectionModel<T, TC>(T source)
        //{
        //    //return new ThreadSaveCollectionModel<ThreadSaveObservableCollection<TE>, TE>() { Collection = source };
        //    return new ThreadSaveCollectionModel<T, TC> { Collection = source };
        //}
    }
}