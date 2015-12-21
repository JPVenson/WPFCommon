using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace JPB.WPFBase.MVVM.ViewModel
{
    public class ActionWaiter
    {
        public ActionWaiter(Action action)
        {
            Action = action;
            Waiter = new ManualResetEventSlim();
        }
        public Action Action { get; private set; }
        public ManualResetEventSlim Waiter { get; private set; }
    }

    public class WaiterResult<T>
    {
        public WaiterResult(ManualResetEventSlim waiter)
        {
            Waiter = waiter;
        }
        public ManualResetEventSlim Waiter { get; private set; }
        public T Result { get; internal set; }
    }

    public class SeriellTaskFactory
    {
        private Thread _thread;

        private volatile bool _working;

        public SeriellTaskFactory()
        {
            ConcurrentQueue = new ConcurrentQueue<ActionWaiter>();
        }

        private ConcurrentQueue<ActionWaiter> ConcurrentQueue { get; set; }

        public ManualResetEventSlim Add(Action action)
        {
            var handler = new ActionWaiter(action);
            ConcurrentQueue.Enqueue(handler);
            StartScheduler();
            return handler.Waiter;
        }

        public WaiterResult<T> AddResult<T>(Func<T> action)
        {
            WaiterResult<T> result = null;
            var handler = new ActionWaiter(() => result.Result = action());
            result = new WaiterResult<T>(handler.Waiter);
            ConcurrentQueue.Enqueue(handler);
            StartScheduler();
            return result;
        }

        public void AddWait(Action action)
        {
            var handler = new ActionWaiter(action);
            ConcurrentQueue.Enqueue(handler);
            StartScheduler();
            handler.Waiter.Wait();
        }

        public T AddWait<T>(Func<T> action)
        {
            T value = default(T);
            var handler = new ActionWaiter(() => value = action());
            ConcurrentQueue.Enqueue(handler);
            StartScheduler();
            handler.Waiter.Wait();
            return value;
        }

        private void StartScheduler()
        {
            if (_working)
                return;

            _working = true;
            _thread = new Thread(Worker);
            _thread.SetApartmentState(ApartmentState.MTA);
            _thread.Start();
        }

        internal void Worker()
        {
            while (ConcurrentQueue.Any())
            {
                ActionWaiter action;
                if (ConcurrentQueue.TryDequeue(out action))
                {
                    using (action.Waiter)
                    {
                        action.Action.Invoke();
                        action.Waiter.Set();
                    }
                }
            }
            _working = false;
        }
    }

    public class AsyncThreadSaveCollection<T>
        : AsyncViewModelBase,
        IEnumerable<T>,
        ICollection<T>
    {
        private readonly object LockObject = new object();
        private readonly ThreadSaveViewModelActor actorHelper;

        private readonly Collection<T> _base;
        private readonly SeriellTaskFactory _tasker;

        public int Count
        {
            get
            {
                return _tasker.AddWait(() => _base.Count);
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((ICollection<T>)_base).IsReadOnly;
            }
        }

        private AsyncThreadSaveCollection(IEnumerable<T> collection, bool copy)
            : this((Dispatcher)null)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            actorHelper = new ThreadSaveViewModelBase();
        }

        public AsyncThreadSaveCollection(IEnumerable<T> collection)
            : this(collection, false)
        {
        }

        public AsyncThreadSaveCollection()
            : this((Dispatcher)null)
        {

        }

        public AsyncThreadSaveCollection(Dispatcher fromThread)
        {
            actorHelper = new ThreadSaveViewModelBase(fromThread);
            _base = new Collection<T>();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _tasker.AddWait(() => _base.GetEnumerator());
        }

        public WaiterResult<IEnumerator<T>> GetEnumeratorAsync()
        {
            return _tasker.AddResult(() => _base.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(T item)
        {
            _tasker.Add(() => _base.Add(item));
        }

        public void Clear()
        {
            _tasker.Add(() => _base.Clear());
        }

        public bool Contains(T item)
        {
            return _tasker.AddWait(() => _base.Contains(item));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _tasker.AddWait(() => _base.CopyTo(array, arrayIndex));
        }

        public bool Remove(T item)
        {
            return _tasker.AddWait(() => _base.Remove(item));
        }
    }
}
