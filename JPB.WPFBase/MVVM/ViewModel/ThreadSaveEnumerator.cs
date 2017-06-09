using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.WPFBase.MVVM.ViewModel
{
    public class ThreadSaveEnumerator<T> : IEnumerator<T>
    {
        private ThreadSaveObservableCollection<T> _collection;
        private int counter;

        public ThreadSaveEnumerator(ThreadSaveObservableCollection<T> collection)
        {
            _collection = collection;
        }

        public T Current
        {
            get; set;
        }

        object IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }

        public bool MoveNext()
        {
	        lock (_collection.SyncRoot)
	        {
				if (_collection.Count > counter + 1)
					return false;
				counter++;
				Current = _collection[counter];
				return true;
			}
        }

        public void Reset()
        {
            counter = 0;
            Current = _collection[counter];
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ThreadSaveEnumerator() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
