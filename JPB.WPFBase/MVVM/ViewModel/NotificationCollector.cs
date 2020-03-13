using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Threading;

namespace JPB.WPFBase.MVVM.ViewModel
{
	internal class NotificationCollector : IDisposable
	{
		private readonly ViewModelBase _vm;

		public NotificationCollector(ViewModelBase vm)
		{
			SendNotifications = new ConcurrentHashSet<string>();
			_vm = vm;
		}

		public ConcurrentHashSet<string> SendNotifications { get; private set; }

		public void Dispose()
		{
			_vm.DeferredNotification = null;
			foreach (var notification in SendNotifications)
			{
				_vm.SendPropertyChanged(notification);
			}
			SendNotifications.Clear();
			SendNotifications.Dispose();
			SendNotifications = null;
		}
	}

	public class ConcurrentHashSet<T> : IDisposable, IEnumerable<T>
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly HashSet<T> _hashSet = new HashSet<T>();

        #region Implementation of ICollection<T> ...ish
        public bool Add(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                return _hashSet.Add(item);
            }
            finally
            {
                if (_lock.IsWriteLockHeld)
                {
	                _lock.ExitWriteLock();
                }
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _hashSet.Clear();
            }
            finally
            {
                if (_lock.IsWriteLockHeld)
                {
	                _lock.ExitWriteLock();
                }
            }
        }

        public bool Contains(T item)
        {
            _lock.EnterReadLock();
            try
            {
                return _hashSet.Contains(item);
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                {
	                _lock.ExitReadLock();
                }
            }
        }

        public bool Remove(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                return _hashSet.Remove(item);
            }
            finally
            {
                if (_lock.IsWriteLockHeld)
                {
	                _lock.ExitWriteLock();
                }
            }
        }

        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _hashSet.Count;
                }
                finally
                {
                    if (_lock.IsReadLockHeld)
                    {
	                    _lock.ExitReadLock();
                    }
                }
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
	            if (_lock != null)
	            {
		            _lock.Dispose();
	            }
            }
        }

        private class ConcurrentHashSetEnumerator : IEnumerator<T>
        {
	        private readonly ConcurrentHashSet<T> _hashset;
	        private readonly IEnumerator<T> _baseListEnumerator;

	        public ConcurrentHashSetEnumerator(ConcurrentHashSet<T> hashset)
	        {
		        _hashset = hashset;
		        _baseListEnumerator = _hashset._hashSet.GetEnumerator();
	        }

	        public void Dispose()
	        {
		        if (_hashset._lock.IsReadLockHeld)
		        {
			        _hashset._lock.ExitReadLock();
		        }
	        }

	        public bool MoveNext()
	        {
		        return _baseListEnumerator.MoveNext();
	        }

	        public void Reset()
	        {
		        _baseListEnumerator.Reset();
	        }

	        public T Current
	        {
		        get { return _baseListEnumerator.Current; }
	        }

	        object IEnumerator.Current
	        {
		        get { return Current; }
	        }
        }

        public IEnumerator<T> GetEnumerator()
        {
	        _lock.EnterReadLock();
            return new ConcurrentHashSetEnumerator(this);
        }

        ~ConcurrentHashSet()
        {
            Dispose(false);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
	        return GetEnumerator();
        }

        #endregion
    }
}