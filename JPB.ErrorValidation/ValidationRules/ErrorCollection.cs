using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using JPB.ErrorValidation.ValidationTyps;

namespace JPB.ErrorValidation.ValidationRules
{
    public abstract class ErrorCollection : IErrorCollectionBase
    {
        private readonly Type _validationType;
        protected readonly ICollection<IValidation> _vallidationErrors;

        protected ErrorCollection(Type validationType)
        {
            _validationType = validationType;
            _vallidationErrors = new ObservableCollection<IValidation>();
        }

        #region IErrorInfoProvider Members

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
            return _validationType;
        }

        public IEnumerable<IValidation> ReturnErrors(string columnName)
        {
            return _vallidationErrors.Where(s => s.ErrorIndicator.Contains(columnName));
        }

        public void Add(IValidation item)
        {
            _vallidationErrors.Add(item);
        }

        public void Add(IEnumerable<IValidation> item)
        {
            foreach (var validation in item)
            {
                _vallidationErrors.Add(validation);
            }
        }

        public void Clear()
        {
            _vallidationErrors.Clear();
        }

        public bool Contains(IValidation item)
        {
            return _vallidationErrors.Contains(item);
        }

        public void CopyTo(IValidation[] array, int arrayIndex)
        {
            _vallidationErrors.CopyTo(array, arrayIndex);
        }

        public bool Remove(IValidation item)
        {
            if (_vallidationErrors.Contains(item))
                return _vallidationErrors.Remove(item);
            return false;
        }

        public IEnumerator<IValidation> GetEnumerator()
        {
            return _vallidationErrors.GetEnumerator();
        }

        #endregion

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_vallidationErrors).GetEnumerator();
        }
    }

    public abstract class ErrorCollection<T> : ErrorCollection
    {
        public ErrorCollection()
            : base(typeof(T))
        {
        }
    }

    //public abstract class ValidationRuleBase : ValidationRuleBaseElement<IValidation>
    //{
    //    protected ValidationRuleBase(Type validationType) : base(validationType)
    //    {
    //    }
    //}

    //public abstract class ValidationRuleBase<T> : ValidationRuleBaseElement<IValidation<T>>
    //{
    //    protected ValidationRuleBase() : base(typeof(T))
    //    {
    //    }
    //}
}