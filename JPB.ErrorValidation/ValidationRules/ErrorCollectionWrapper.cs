using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using JPB.ErrorValidation.ValidationTyps;

namespace JPB.ErrorValidation.ValidationRules
{
	/// <summary>
	///     Defines a base class for the Collection of Errors
	/// </summary>
	public abstract class ErrorCollectionWrapper : IErrorCollectionBase
	{
		private readonly Type _validationType;

		protected ErrorCollectionWrapper(Type validationType)
		{
			_validationType = validationType ?? throw new ArgumentNullException(nameof(validationType));
		}

		protected abstract ICollection<IValidation> Errors { get; }

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable) Errors).GetEnumerator();
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			CollectionChanged?.Invoke(this, e);
		}

		#region IErrorInfoProvider Members

		public int Count
		{
			get { return Errors.Count; }
		}

		public bool IsReadOnly
		{
			get { return Errors.IsReadOnly; }
		}

		public Type RetrunT()
		{
			return _validationType;
		}

		public IEnumerable<IValidation> FilterErrors(string columnName)
		{
			return Errors.Where(s => s.ErrorIndicator.Contains(columnName) || !s.ErrorIndicator.Any());
		}

		public void Add(IValidation item)
		{
			Errors.Add(item);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
		}

		public void Add(IEnumerable<IValidation> item)
		{
			foreach (var validation in item)
			{
				Add(validation);
			}
		}

		public void Clear()
		{
			Errors.Clear();
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public bool Contains(IValidation item)
		{
			return Errors.Contains(item);
		}

		public void CopyTo(IValidation[] array, int arrayIndex)
		{
			Errors.CopyTo(array, arrayIndex);
		}

		public bool Remove(IValidation item)
		{
			if (Errors.Contains(item))
			{
				if (Errors.Remove(item))
				{
					OnCollectionChanged(
						new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
					return true;
				}
			}

			return false;
		}

		public IEnumerator<IValidation> GetEnumerator()
		{
			return Errors.GetEnumerator();
		}

		#endregion
	}
}