//using System;
//using System.Collections;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace JPB.WPFBase.MVVM.ViewModel
//{
//    public class ThreadSaveEnumerator<T> : IEnumerator<T>
//    {
//        private IEnumerable<T> _collection;
//        private int _counter;

//        public ThreadSaveEnumerator(T[] collection)
//        {
//	        _collection = collection;
//        }

//        public T Current
//        {
//            get; set;
//        }

//        object IEnumerator.Current
//        {
//            get
//            {
//                return this.Current;
//            }
//        }

//        public bool MoveNext()
//        {
//	        lock (_collection.SyncRoot)
//	        {
//				if (_collection.Count > _counter + 1)
//					return false;
//				_counter++;
//				Current = _collection[_counter];
//				return true;
//			}
//        }

//        public void Reset()
//        {
//            _counter = 0;
//            Current = _collection[_counter];
//        }

//        #region IDisposable Support
//        private bool disposedValue = false; // To detect redundant calls

//        protected virtual void Dispose(bool disposing)
//        {
//            if (!disposedValue)
//            {
//                if (disposing)
//                {
//	                _collection = null;
//	                _counter = -1;
//                }

//                disposedValue = true;
//            }
//        }

//        public void Dispose()
//        {
//            Dispose(true);
//        }
//        #endregion
//    }
//}
