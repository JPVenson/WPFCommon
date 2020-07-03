using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace JPB.WPFBase.MVVM.ViewModel
{
	/// <summary>
	///     Extends the <see cref="ThreadSaveObservableCollection{T}" /> with the IBindingList interface
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class BindingListThreadSaveObservableCollection<T> : ThreadSaveObservableCollection<T>,
		IBindingList
	{
		static BindingListThreadSaveObservableCollection()
		{
			var defaultConstructor = typeof(T).GetConstructors(BindingFlags.CreateInstance | BindingFlags.Public)
				.FirstOrDefault(e => !e.GetParameters().Any());
			if (defaultConstructor != null)
			{
				Factory = () => (T)defaultConstructor.Invoke(null);
			}
		}

		public static Func<T> Factory { get; set; }

		private readonly IList<PropertyDescriptor> _searchIndexes = new List<PropertyDescriptor>();

		/// <summary>
		/// </summary>
		public BindingListThreadSaveObservableCollection()
		{
		}

		/// <inheritdoc />
		public object AddNew()
		{
			if (Factory == null)
			{
				throw new NotSupportedException($"AllowNew for type '{typeof(T)}' is invalid");
			}

			return Factory.Invoke();
		}

		/// <inheritdoc />
		public void AddIndex(PropertyDescriptor property)
		{
			_searchIndexes.Add(property);
			OnListChanged(new ListChangedEventArgs(ListChangedType.PropertyDescriptorAdded, property));
		}

		/// <inheritdoc />
		public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
		{
			IEnumerable<T> sortedDict = null;
			if (direction == ListSortDirection.Ascending)
			{
				sortedDict = this.OrderBy(e => property.GetValue(e));
			}
			else
			{
				sortedDict = this.OrderByDescending(e => property.GetValue(e));
			}

			ThreadSaveAction(() =>
			{
				var enumerable = sortedDict.ToArray();
				for (var index = 0; index < enumerable.Length; index++)
				{
					var itemAt = enumerable[index];
					if (itemAt.Equals(this[index]))
					{
						continue;
					}

					ReplaceItem(index, itemAt);
				}
			});
		}

		/// <inheritdoc />
		public int Find(PropertyDescriptor property, object key)
		{
			for (var index = 0; index < this.Count; index++)
			{
				var item = this[index];
				var findValue = property.GetValue(item);
				if (findValue == key || findValue?.Equals(key) == true)
				{
					return index;
				}
			}

			return -1;
		}

		/// <inheritdoc />
		public void RemoveIndex(PropertyDescriptor property)
		{
			_searchIndexes.Remove(property);
			OnListChanged(new ListChangedEventArgs(ListChangedType.PropertyDescriptorDeleted, property));
		}

		/// <inheritdoc />
		public void RemoveSort()
		{
		}

		/// <inheritdoc />
		public bool AllowNew
		{
			get
			{
				return Factory != null;
			}
		}

		/// <inheritdoc />
		public bool AllowEdit { get; set; }

		/// <inheritdoc />
		public bool AllowRemove { get; set; }

		/// <inheritdoc />
		public bool SupportsChangeNotification { get; } = true;

		/// <inheritdoc />
		public bool SupportsSearching { get; }

		/// <inheritdoc />
		public bool SupportsSorting { get; }

		/// <inheritdoc />
		public bool IsSorted { get; }

		/// <inheritdoc />
		public PropertyDescriptor SortProperty { get; }

		/// <inheritdoc />
		public ListSortDirection SortDirection { get; }

		/// <inheritdoc />
		public event ListChangedEventHandler ListChanged;

		protected virtual void OnListChanged(ListChangedEventArgs e)
		{
			ListChanged?.Invoke(this, e);
		}

		/// <inheritdoc />
		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			base.OnCollectionChanged(e);
			var bindingCollectionHandler = ListChanged;
			if (bindingCollectionHandler != null)
			{
				ListChangedEventArgs bindingCollectionArgs;
				switch (e.Action)
				{
					case NotifyCollectionChangedAction.Add:
						bindingCollectionArgs = new ListChangedEventArgs(ListChangedType.ItemAdded, e.NewStartingIndex);
						break;
					case NotifyCollectionChangedAction.Remove:
						bindingCollectionArgs =
							new ListChangedEventArgs(ListChangedType.ItemDeleted, e.NewStartingIndex);
						break;
					case NotifyCollectionChangedAction.Replace:
						bindingCollectionArgs = new ListChangedEventArgs(ListChangedType.ItemChanged,
							e.NewStartingIndex, e.OldStartingIndex);
						break;
					case NotifyCollectionChangedAction.Move:
						bindingCollectionArgs = new ListChangedEventArgs(ListChangedType.ItemMoved, e.NewStartingIndex,
							e.OldStartingIndex);
						break;
					case NotifyCollectionChangedAction.Reset:
						bindingCollectionArgs = new ListChangedEventArgs(ListChangedType.Reset, 0);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				bindingCollectionHandler(this, bindingCollectionArgs);
			}
		}
	}
}