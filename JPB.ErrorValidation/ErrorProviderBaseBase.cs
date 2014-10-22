using System;
using System.Collections;
using System.Linq;
using System.Xml.Serialization;
using JPB.ErrorValidation.ValidationTyps;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.ErrorValidation
{
    public class ErrorProviderBaseBase<T, TE> : AsyncViewModelBase
        where T : class
        where TE : class, IErrorInfoProvider<T>, new()
    {
        private string _error;

        public string Error
        {
            get
            {
                if (Validate)
                    return _error;
                return string.Empty;
            }
            set
            {
                _error = value;

                base.ThreadSaveAction(() =>
                {
                    SendPropertyChanged("Error");
                });
            }
        }

        protected ErrorProviderBaseBase()
        {
            if (ErrorObserver<T>.Instance.GetProviderViaType() == null)
                ErrorObserver<T>.Instance.RegisterErrorProvider(new TE());
            //TODO add async validation

            //ErrorInfoProviderSimpleAccessAdapter.Errors.CollectionChanged += ErrorsOnCollectionChanged;

            AddTypeToText = true;
            Validation = ValidationLogic.BreakAtFirstFail;
            Validate = true;
            DefaultNoError = new NoError<T> { ErrorText = "OK" };
        }

        protected bool Validate { get; set; }
        protected ValidationLogic Validation { get; set; }

        [XmlIgnore]
        public IErrorInfoProvider<T> ErrorInfoProviderSimpleAccessAdapter
        {
            get { return ErrorObserver<T>.Instance.GetProviderViaType(); }
        }

        public bool HasError
        {
            get { return ErrorInfoProviderSimpleAccessAdapter.HasError; }
        }

        public bool AddTypeToText { get; set; }

        [XmlIgnore]
        public NoError<T> DefaultNoError { get; set; }

        public bool HasErrors { get { return HasError; } }

        public enum ValidationLogic
        {
            BreakAtFirstFail,
            BreakAtFirstFailButRunAllWarnings,
            RunThroghAll
        }

        public void ForceRefresh()
        {
            if (Validate)
                foreach (var error in ErrorInfoProviderSimpleAccessAdapter.SelectMany(s => s.ErrorIndicator).Distinct())
                    ObManage(error, this as T);
        }

        public IValidation<T> GetError(string columnName, T obj)
        {
            return ObManage(columnName, obj);
        }

        private IValidation<T> ObManage(string errorIndicator, T obj)
        {
            IValidation<T> err = null;

            if (!Validate)
            {
                if (ErrorInfoProviderSimpleAccessAdapter.Errors.Any())
                    ErrorInfoProviderSimpleAccessAdapter.Errors.Clear();
                Error = string.Empty;
                return err;
            }

            var refference = ErrorObserver<T>.Instance.GetProviderViaType().Errors;

            if (refference.Any(s => s is NoError<T>))
            {
                IValidation<T> validation = refference.First(s => s is NoError<T>);
                try
                {
                    refference.Remove(validation);
                }
                catch (Exception)
                {
                    throw;
                }
            }

            var listOfErrors =
                ErrorObserver<T>.Instance.GetProviderViaType()
                    .Where(s => s.ErrorIndicator.Contains(errorIndicator) || !s.ErrorIndicator.Any()).ToArray();

            switch (Validation)
            {
                case ValidationLogic.RunThroghAll:
                    foreach (var error in listOfErrors)
                    {
                        if (ManageValidationRule(obj, error))
                        {
                            err = error;
                        }
                    }
                    break;
                case ValidationLogic.BreakAtFirstFail:
                    foreach (var item in listOfErrors)
                    {
                        if (ManageValidationRule(obj, item))
                        {
                            err = item;
                            break;
                        }
                    }
                    break;
                case ValidationLogic.BreakAtFirstFailButRunAllWarnings:
                    var ofErrors = listOfErrors as IValidation<T>[] ?? listOfErrors.ToArray();

                    foreach (var warning in listOfErrors.Where(s => s is Warning<T>))
                    {
                        ManageValidationRule(obj, warning);
                    }

                    foreach (var item in ofErrors.Where(s => s is Error<T>))
                    {
                        var isFailed = ManageValidationRule(obj, item);
                        if (isFailed)
                        {
                            break;
                        }
                    }
                    break;
            }

            if (!refference.Any(s => s is Error<T> || s is Warning<T>))
            {
                Error = string.Empty;
                refference.Add(DefaultNoError);
            }

            if (err != null && !err.ErrorText.Equals(Error))
            {
                Error = err.ErrorText;
            }
            return err;
        }

        private bool ManageValidationRule(T obj, IValidation<T> item)
        {
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
            if (conditionresult && !ErrorInfoProviderSimpleAccessAdapter.Errors.Contains(item))
            {
                if (AddTypeToText)
                {
                    if (item.ErrorText.StartsWith(item.ErrorType))
                        item.ErrorText = string.Format("{0} : {1}", item.ErrorType, item.ErrorText);
                }
                ErrorInfoProviderSimpleAccessAdapter.Errors.Add(item);
                Error = item.ErrorText;
            }

            // Bedingung ist flasch und error ist in der liste der angezeigten errors
            if (!conditionresult && ErrorInfoProviderSimpleAccessAdapter.Errors.Contains(item))
                ErrorInfoProviderSimpleAccessAdapter.Errors.Remove(item);
            else if (ErrorInfoProviderSimpleAccessAdapter.Errors.Contains(item))
                Error = item.ErrorText;

            return conditionresult;
        }
    }
}