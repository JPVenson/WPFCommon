using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using JPB.ErrorValidation.ValidationTyps;
using JPB.Tasking.TaskManagement.Threading;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.ErrorValidation
{
    public abstract class ErrorProviderBase<T, TE> :
        AsyncViewModelBase,
        IErrorProviderBase<T>
        where T : class
        where TE : class, IErrorProvider<T>, new()
    {
        private string _error;
        private readonly int _maximumErrorValidationConcurrency;

        public SingelSeriellTaskFactory TaskFactory { get; set; }

        protected bool Validate { get; set; }

        protected ValidationLogic Validation { get; set; }

        public enum ValidationLogic
        {
            BreakAtFirstFail,
            BreakAtFirstFailButRunAllWarnings,
            RunThroghAll
        }

        public int MaximumErrorValidationConcurrency
        {
            get { return _maximumErrorValidationConcurrency; }
        }

        protected ErrorProviderBase(int maxiumConcurrency)
        {
            _maximumErrorValidationConcurrency = maxiumConcurrency;
            TaskFactory = new SingelSeriellTaskFactory();

            if (ErrorObserver<T>.Instance.GetProviderViaType() == null)
                ErrorObserver<T>.Instance.RegisterErrorProvider(new TE());
            //TODO add async validation

            PropertyChanged += OnPropertyChanged;
            //ErrorProviderSimpleAccessAdapter.Errors.CollectionChanged += ErrorsOnCollectionChanged;

            AddTypeToText = true;
            Validation = ValidationLogic.BreakAtFirstFail;
            Validate = true;
            DefaultNoError = new NoError<T> { ErrorText = "OK" };
        }

        private void ErrorsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            var items = new List<IValidation<T>>();

            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Add)
                items.AddRange(notifyCollectionChangedEventArgs.NewItems.Cast<IValidation<T>>());
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Remove)
                items.AddRange(notifyCollectionChangedEventArgs.OldItems.Cast<IValidation<T>>());

            TaskFactory.Add(() =>
            {
                foreach (var validation in items.SelectMany(s => s.ErrorIndicator).Distinct())
                {
                    this.OnErrorsChanged(validation);
                }
            }, sender);
        }

        protected ErrorProviderBase()
            : this(Environment.ProcessorCount)
        {

        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            //validate Async
            if (!Validate)
                return;
            TaskFactory.Add(() =>
            {
                ObManage(propertyChangedEventArgs.PropertyName, this as T);
            }, propertyChangedEventArgs.PropertyName);
        }

        #region IErrorProviderBase<T> Members

        [XmlIgnore]
        public IErrorProvider<T> ErrorProviderSimpleAccessAdapter
        {
            get { return ErrorObserver<T>.Instance.GetProviderViaType(); }
        }

        public bool HasError
        {
            get { return ErrorProviderSimpleAccessAdapter.HasError; }
        }

        public bool AddTypeToText { get; set; }

        public string Error
        {
            get
            {
                if (Validate)
                    return _error;
                return string.Empty;
            }
        }

        [XmlIgnore]
        public NoError<T> DefaultNoError { get; set; }

        string IDataErrorInfo.Error
        {
            get { return Error; }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get { return GetError(columnName, this as T); }
        }

        public void ForceRefresh()
        {
            if (Validate)
                foreach (var error in ErrorProviderSimpleAccessAdapter.SelectMany(s => s.ErrorIndicator).Distinct())
                    ObManage(error, this as T);
        }

        public string GetError(string columnName, T obj)
        {
            return ObManage(columnName, obj);
        }

        #endregion

        private string ObManage(string errorIndicator, T obj)
        {
            if (!Validate)
            {
                if (ErrorProviderSimpleAccessAdapter.Errors.Any())
                    ErrorProviderSimpleAccessAdapter.Errors.Clear();
                _error = string.Empty;
                return _error;
            }

            List<IValidation<T>> refference = ErrorObserver<T>.Instance.GetProviderViaType().Errors.ToList();

            if (refference.Any(s => s is NoError<T>))
            {
                IValidation<T> validation = refference.First(s => s is NoError<T>);
                refference.Remove(validation);
            }

            IEnumerable<IValidation<T>> listOfErrors =
                ErrorObserver<T>.Instance.GetProviderViaType()
                    .Where(s => s.ErrorIndicator.Contains(errorIndicator) || !s.ErrorIndicator.Any());

            var errortext = "";
            switch (Validation)
            {
                case ValidationLogic.RunThroghAll:
                    errortext =
                        listOfErrors.Select(vaildValidation => ManageError(obj, vaildValidation))
                            .Aggregate(string.Empty, (current, error) => string.IsNullOrEmpty(error) ? current : error);
                    break;
                case ValidationLogic.BreakAtFirstFail:
                    foreach (var item in listOfErrors)
                    {
                        errortext = ManageError(obj, item);
                        if (errortext != string.Empty)
                        {
                            break;
                        }
                    }
                    break;
                case ValidationLogic.BreakAtFirstFailButRunAllWarnings:
                    var ofErrors = listOfErrors as IValidation<T>[] ?? listOfErrors.ToArray();

                    foreach (var item in ofErrors.Where(s => s is Warning<T>))
                    {
                        errortext += ManageError(obj, item);
                    }

                    foreach (var item in ofErrors.Where(s => s is Error<T>))
                    {
                        errortext = ManageError(obj, item);
                        if (errortext != string.Empty)
                        {
                            break;
                        }
                    }
                    break;
            }

            if (!refference.Any(s => s is Error<T> || s is Warning<T>))
                refference.Add(DefaultNoError);

            if (errortext != null && !errortext.Equals(_error))
            {
                _error = errortext;
                SendPropertyChanged("Error");
            }
            return errortext;
        }

        private string ManageError(T obj, IValidation<T> item)
        {
            string errortext = string.Empty;
            bool conditionresult;
            try
            {
                conditionresult = item.Condition(obj);
            }
            catch (Exception)
            {
                conditionresult = true;
            }

            // Bedingung ist wahr und error ist nicht in der liste der angezeigten errors
            if (conditionresult && !ErrorProviderSimpleAccessAdapter.Errors.Contains(item))
            {
                if (AddTypeToText)
                {
                    if (item.ErrorText.StartsWith(item.ErrorType))
                        item.ErrorText = string.Format("{0} : {1}", item.ErrorType, item.ErrorText);
                }
                ErrorProviderSimpleAccessAdapter.Errors.Add(item);
                errortext = item.ErrorText;
            }

            // Bedingung ist flasch und error ist in der liste der angezeigten errors
            if (!conditionresult && ErrorProviderSimpleAccessAdapter.Errors.Contains(item))
                ErrorProviderSimpleAccessAdapter.Errors.Remove(item);

            else if (ErrorProviderSimpleAccessAdapter.Errors.Contains(item))
                errortext = item.ErrorText;

            return errortext;
        }

        public IEnumerable GetErrors(string propertyName)
        {
            return ErrorProviderSimpleAccessAdapter.Errors.Select(s => s.ErrorText);
        }

        public bool HasErrors { get { return HasError; } }
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected virtual void OnErrorsChanged(DataErrorsChangedEventArgs e)
        {
            var handler = ErrorsChanged;
            if (handler != null)
                base.ThreadSaveAction(() => handler(this, e));
        }

        protected virtual void OnErrorsChanged(string propname)
        {
            var handler = ErrorsChanged;
            if (handler != null)
                base.ThreadSaveAction(() => handler(this, new DataErrorsChangedEventArgs(propname)));
        }
    }
}