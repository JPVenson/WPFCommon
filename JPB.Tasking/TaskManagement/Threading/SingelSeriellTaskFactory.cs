using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JPB.Tasking.TaskManagement.Threading
{
	[Obsolete]
    public class SingelSeriellTaskFactory : IDisposable
    {
        private bool _isDisposing;
        private Thread _thread;
        private readonly int _maxRunPerKey;
        private bool _working;
        private readonly object _lock = new object();

        #region Implementation of IDisposable

        /// <summary>
        ///     Führt anwendungsspezifische Aufgaben durch, die mit der Freigabe, der Zurückgabe oder dem Zurücksetzen von nicht
        ///     verwalteten Ressourcen zusammenhängen.
        /// </summary>
        public void Dispose()
        {
            _isDisposing = true;
        }

        #endregion

        public SingelSeriellTaskFactory(int maxRunPerKey = 1)
        {
            _maxRunPerKey = maxRunPerKey;
            ConcurrentQueue = new ConcurrentQueue<Tuple<Action, object>>();
        }

        public ConcurrentQueue<Tuple<Action, object>> ConcurrentQueue { get; set; }

        public bool Add(Action action, object key)
        {
            return Add(action, key, _maxRunPerKey);
        }


        public bool Add(Action action, object key, int max)
        {
            //nested usage is not allowed
            if (_thread == Thread.CurrentThread)
            {
                action();
                return true;
            }

            if (key != null && ConcurrentQueue.Count(s => s.Item2.Equals(key)) > max)
            {
                //var task = new Task(() =>
                //{
                //    if (ConcurrentQueue.Any(s => s.Item2 == key))
                //        return;
                //    ConcurrentQueue.Enqueue(new Tuple<Action, object>(action, key));
                //    StartScheduler();
                //});
                //task.Start();
                return false;
            }

            ConcurrentQueue.Enqueue(new Tuple<Action, object>(action, key));
            StartScheduler();
            return true;
        }

        private void StartScheduler()
        {
            if (_working)
                return;
            lock (_lock)
            {
                if (_working)
                    return;
                _working = true;
                _thread = new Thread(Worker);
                _thread.Name = "SSTF_" + GetHashCode();
                _thread.SetApartmentState(ApartmentState.MTA);
                _thread.Start();
            }
        }

        internal void Worker()
        {
            while (ConcurrentQueue.Any() && !_isDisposing)
            {
                Tuple<Action, object> action;
                if (ConcurrentQueue.TryDequeue(out action))
                {
                    action.Item1.Invoke();
                    action = null;
                }
            }
            _working = false;
        }
    }
}