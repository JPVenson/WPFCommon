using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using JPB.ErrorValidation.ValidationTyps;
using JPB.Tasking.TaskManagement.Threading;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.ErrorValidation
{
    public class AsyncErrorProviderBase<T, TE> :
        ErrorProviderBase<T, TE>,
        INotifyDataErrorInfo
        where T : class
        where TE : class, IErrorInfoProvider<T>, new()
    {
        public AsyncErrorProviderBase()
        {
            base.ErrorInfoProviderSimpleAccessAdapter.CollectionChanged += Errors_CollectionChanged;

            ErrorMapper = new ConcurrentDictionary<string, List<IValidation<T>>>();

            PropertyChanged += AsyncErrorProviderBase_PropertyChanged;

            foreach (var validation in ErrorInfoProviderSimpleAccessAdapter)
            {
                var errorList = new List<IValidation<T>>();

                foreach (var validationKey in validation.ErrorIndicator)
                {
                    ErrorMapper.GetOrAdd(validationKey, errorList);
                }
            }
        }


        private SingelSeriellTaskFactory _errorFactory = new SingelSeriellTaskFactory();
        private bool _isValidating;

        public bool IsValidating
        {
            get { return _isValidating; }
            set
            {
                _isValidating = value;
                base.ThreadSaveAction(() => SendPropertyChanged(() => IsValidating));
            }
        }

        void AsyncErrorProviderBase_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ErrorMapper.ContainsKey(e.PropertyName))
            {
                ValidateAsync(e.PropertyName);
            }
        }

        public ConcurrentDictionary<string, List<IValidation<T>>> ErrorMapper { get; set; }

        void Errors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var propertyName = string.Empty;

            IValidation<T>[] listOfValidators;

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                listOfValidators = e.NewItems.Cast<IValidation<T>>().ToArray();
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                listOfValidators = e.OldItems.Cast<IValidation<T>>().ToArray();
            }
            else
            {
                throw new NotSupportedException("Other actions then Add or Remove are not supported");
            }

            if (!listOfValidators.Any())
                return;

            var firstOrDefault = listOfValidators.FirstOrDefault();
            if (firstOrDefault == null || firstOrDefault.ErrorIndicator == null)
                return;

            var orDefault = firstOrDefault.ErrorIndicator.FirstOrDefault();
            if (orDefault != null)
                propertyName = orDefault;

            OnErrorsChanged(new DataErrorsChangedEventArgs(propertyName));
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected virtual void OnErrorsChanged(DataErrorsChangedEventArgs e)
        {
            var handler = ErrorsChanged;
            if (handler != null) handler(this, e);
        }

        public IEnumerable GetErrors(string propertyName)
        {
            List<IValidation<T>> errorJob = null;
            ErrorMapper.TryGetValue(propertyName, out errorJob);
            return errorJob;
        }

        private void ValidateAsync(string propertyName)
        {
            _errorFactory.Add(() =>
            {
                List<IValidation<T>> errorJob = null;
                ErrorMapper.TryGetValue(propertyName, out errorJob);
                base.GetError(propertyName, this as T);

                if (errorJob != null)
                {
                    errorJob.Clear();
                    errorJob.AddRange(base.ErrorInfoProviderSimpleAccessAdapter.Errors);
                }
            }, propertyName);
        }
    }
}