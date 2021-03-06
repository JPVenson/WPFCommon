﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using JPB.WPFToolsAwesome.Error.ValidationTypes;

namespace JPB.WPFToolsAwesome.Error.ValidationRules
{
	/// <summary>
	///     Defines a base class for the Collection of Errors
	/// </summary>
	public abstract class ErrorCollectionWrapper : IErrorCollectionBase
	{
		public Type ValidationType { get; }

		protected ErrorCollectionWrapper(Type validationType)
		{
			ValidationType = validationType ?? throw new ArgumentNullException(nameof(validationType));
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
		
		public IEnumerable<IValidation> FilterErrors(string fieldName)
		{
			return Errors.Where(s => s.ErrorIndicator.Contains(fieldName) || !s.ErrorIndicator.Any());
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