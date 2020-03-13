using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.Tasking.TaskManagement.Threading;

namespace JPB.Tasking.Misc.Cache
{
	public enum CacheWriteOperation
	{
		Add,
		Remove
	}

	public struct ReadWriteState<T>
	{
		public ReadWriteState(T item) : this(item, false)
		{
		}

		public ReadWriteState(T item, bool committed)
		{
			Item = item;
			Committed = committed;
		}

		public T Item { get; set; }
		public bool Committed { get; set; }
	}

	public abstract class ReadWriteCache<T>
	{
		public ReadWriteCache()
		{
			_dispatcher = new BlockingTaskDispatcher(false);
			_localCache = new ConcurrentDictionary<T, ReadWriteState<T>>();
		}

		private ConcurrentDictionary<T, ReadWriteState<T>> _localCache;
		private readonly BlockingTaskDispatcher _dispatcher;

		public abstract Task<bool> WriteOperation(T item, CacheWriteOperation operation);

		public void Add(T item, bool immediateCommit = true)
		{
			if (immediateCommit)
			{
				_localCache.TryAdd(item, new ReadWriteState<T>(item));
			}
			_dispatcher.EnqueueNonBlocking(async () =>
			{
				var writeOperation = await WriteOperation(item, CacheWriteOperation.Add);
				if (writeOperation)
				{
					if (!immediateCommit)
					{
						_localCache.TryAdd(item, new ReadWriteState<T>(item));
					}
					_localCache[item] = new ReadWriteState<T>(item, true);
				}
				else
				{
					_localCache.TryRemove(item, out _);
				}
			});
		}

		public void Remove(T item, bool immediateCommit = false)
		{
			if (immediateCommit)
			{
				if (!_localCache.TryRemove(item, out _))
				{
					return;
				}
			}
			else
			{
				_localCache[item] = new ReadWriteState<T>(item, false);	
			}
			_dispatcher.EnqueueNonBlocking(async () =>
			{
				var writeOperation = await WriteOperation(item, CacheWriteOperation.Remove);
				if (writeOperation)
				{
					_localCache.TryRemove(item, out _);
				}
				else
				{
					_localCache[item] = new ReadWriteState<T>(item, true);
				}
			});
		}

		public async Task<IEnumerable<T>> Read(bool awaitWrites = true)
		{
			if (awaitWrites)
			{
				IEnumerable<T> cache = null;
				await _dispatcher.EnqueueAsync(() =>
				{
					cache = _localCache.Select(f => f.Key).ToArray();
				});
				return cache;
			}
			else
			{
				return _localCache.Where(e => e.Value.Committed).Select(f => f.Key).ToArray();
			}
		}
	}
}
