using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using JPB.ErrorValidation.ValidationTyps;
using JPB.ErrorValidation.ViewModelProvider.Base;
using JPB.Tasking.TaskManagement.Threading;

namespace JPB.ErrorValidation.ViewModelProvider
{
    /// <summary>
    /// Provides the INotifyDataErrorInfo Interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TE"></typeparam>
    public class AsyncErrorProviderBase<T> :
        ErrorProviderBase<T>,
        INotifyDataErrorInfo where T : class, IErrorCollectionBase, new()
    {
        public AsyncErrorProviderBase()
        {
            base.CollectionChanged += Errors_CollectionChanged;

            ErrorMapper = new ConcurrentDictionary<string, List<IValidation>>();

            PropertyChanged += AsyncErrorProviderBase_PropertyChanged;

            foreach (var validation in base.UserErrors)
            {
                var errorList = new List<IValidation>();

                foreach (var validationKey in validation.ErrorIndicator)
                {
                    ErrorMapper.GetOrAdd(validationKey, errorList);
                }
            }
        }

        private readonly SingelSeriellTaskFactory _errorFactory = new SingelSeriellTaskFactory();
        private int _workerCount = 0;

        public bool IsValidating
        {
            get { return _workerCount > 0; }
        }

        void AsyncErrorProviderBase_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ErrorMapper.ContainsKey(e.PropertyName))
            {
                ValidateAsync(e.PropertyName);
            }
        }

        public ConcurrentDictionary<string, List<IValidation>> ErrorMapper { get; set; }

        void Errors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var propertyName = string.Empty;

            IValidation[] listOfValidators;

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                listOfValidators = e.NewItems.Cast<IValidation>().ToArray();
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                listOfValidators = e.OldItems.Cast<IValidation>().ToArray();
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
            List<IValidation> errorJob = null;
            ErrorMapper.TryGetValue(propertyName, out errorJob);
            return errorJob;
        }

        private void ValidateAsync(string propertyName)
        {
            _errorFactory.Add(() =>
            {
                try
                {
                    _workerCount++;
                    base.ThreadSaveAction(() => SendPropertyChanged(() => IsValidating));
                    List<IValidation> errorJob = null;
                    ErrorMapper.TryGetValue(propertyName, out errorJob);
                    var failed = base.GetError(propertyName, this);

                    if (errorJob != null)
                    {
                        base.BeginThreadSaveAction(() =>
                        {
                            errorJob.Clear();
                            errorJob.AddRange(failed);
                        });
                    }
                }
                finally
                {
                    _workerCount--;
                    base.ThreadSaveAction(() => SendPropertyChanged(() => IsValidating));
                }

            }, propertyName);
        }

        public bool HasErrors
        {
            get { return base.HasError; }
        }
    }
}