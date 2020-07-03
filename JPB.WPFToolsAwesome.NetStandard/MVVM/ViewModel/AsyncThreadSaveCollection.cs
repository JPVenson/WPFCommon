using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace JPB.WPFToolsAwesome.MVVM.ViewModel
{
	public class AsyncThreadSaveCollection<T>
		: AsyncViewModelBase,
			IEnumerable<T>,
			ICollection<T>
	{
		private readonly object _lockObject = new object();

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
			{
				throw new ArgumentNullException("collection");
			}
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
			: base(fromThread)
		{
			_base = new Collection<T>();
			_tasker = new SeriellTaskFactory();

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

		public async Task AddAsync(T item)
		{
			await _tasker.AddAsync(() => _base.Add(item));
		}

		public async Task ClearAsync()
		{
			await _tasker.AddAsync(() => _base.Clear());
		}

		public async Task<bool> ContainsAsync(T item)
		{
			return await _tasker.AddWaitAsync(() => _base.Contains(item));
		}

		public async Task CopyToAsync(T[] array, int arrayIndex)
		{
			await _tasker.AddWaitAsync(() => _base.CopyTo(array, arrayIndex));
		}

		public async Task<bool> RemoveAsync(T item)
		{
			return await _tasker.AddWaitAsync(() => _base.Remove(item));
		}
	}
}