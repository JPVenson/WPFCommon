﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JPB.ErrorValidation.ValidationTyps;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.ErrorValidation.ValidationRules
{
    public abstract class ValidationRuleBase<T> : IErrorInfoProvider<T>
    {
        private readonly ICollection<IValidation<T>> _vallidationErrors = new ObservableCollection<IValidation<T>>();

        protected ValidationRuleBase()
        {
            Errors = new ThreadSaveObservableCollection<IValidation<T>>();
        }

        #region IErrorInfoProvider<T> Members

        public virtual bool HasError
        {
            get
            {
                if (Errors.Any())
                    return Errors.Any(s => s is Error<T>);
                return false;
            }
        }

        public bool WarningAsFailure { get; set; }

        public ThreadSaveObservableCollection<IValidation<T>> Errors { get; set; }

        public IEnumerable<IValidation<T>> Warnings
        {
            get { return Errors.Where(s => s is Warning<T>); }
        }

        public NoError<T> DefaultNoError { get; set; }

        public int Count
        {
            get { return _vallidationErrors.Count; }
        }

        public bool IsReadOnly
        {
            get { return _vallidationErrors.IsReadOnly; }
        }

        public Type RetrunT()
        {
            return typeof (T);
        }

        public IEnumerable<IValidation<T>> RetrunErrors(string columnName)
        {
            return _vallidationErrors.Where(s => s.ErrorIndicator.Contains(columnName));
        }

        public void Add(IValidation<T> item)
        {
            _vallidationErrors.Add(item);
        }

        public void Clear()
        {
            _vallidationErrors.Clear();
        }

        public bool Contains(IValidation<T> item)
        {
            return _vallidationErrors.Contains(item);
        }

        public void CopyTo(IValidation<T>[] array, int arrayIndex)
        {
            _vallidationErrors.CopyTo(array, arrayIndex);
        }

        public bool Remove(IValidation<T> item)
        {
            if (_vallidationErrors.Contains(item))
                return _vallidationErrors.Remove(item);
            return false;
        }

        public IEnumerator<IValidation<T>> GetEnumerator()
        {
            return _vallidationErrors.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _vallidationErrors.GetEnumerator();
        }

        #endregion
    }
}