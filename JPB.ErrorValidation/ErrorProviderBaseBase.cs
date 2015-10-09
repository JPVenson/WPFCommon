using System;
using System.Collections;
using System.Linq;
using System.Windows.Threading;
using System.Xml.Serialization;
using JPB.ErrorValidation.ValidationTyps;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.ErrorValidation
{
    public class ErrorProviderBase<T, TE> :
        AsyncViewModelBase,
        IErrorProviderBase where T : class
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
                if (value == _error)
                    return;

                _error = value;

                base.ThreadSaveAction(() =>
                {
                    SendPropertyChanged("Error");
                });
            }
        }

        protected ErrorProviderBase(Dispatcher disp)
            : base(disp)
        {
            this.Init();
        }

        protected ErrorProviderBase()
        {
            this.Init();
        }

        public void Init()
        {
            if (ErrorObserver<T>.Instance.GetProviderViaType() == null)
                ErrorObserver<T>.Instance.RegisterErrorProvider(new TE());

            MessageFormat = "{0} - {1}";
            Validation = ValidationLogic.BreakAtFirstFail;
            Validate = true;
        }

        public bool Validate { get; set; }

        [XmlIgnore]
        protected ValidationLogic Validation { get; set; }

        [XmlIgnore]
        public IErrorInfoProvider<T> ErrorInfoProviderSimpleAccessAdapter
        {
            get { return ErrorObserver<T>.Instance.GetProviderViaType(); }
        }

        [XmlIgnore]
        public bool HasError
        {
            get { return ErrorInfoProviderSimpleAccessAdapter.HasError; }
        }

        public string MessageFormat { get; set; }

        [XmlIgnore]
        [Obsolete]
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

            var refference = ErrorInfoProviderSimpleAccessAdapter.Errors;

            var errTemplates =
                ErrorInfoProviderSimpleAccessAdapter
                    .Where(s => s.ErrorIndicator.Contains(errorIndicator) || !s.ErrorIndicator.Any()).ToArray();

            switch (Validation)
            {
                case ValidationLogic.RunThroghAll:
                    foreach (var error in errTemplates)
                    {
                        if (ManageValidationRule(obj, error))
                        {
                            err = error;
                        }
                    }
                    break;
                case ValidationLogic.BreakAtFirstFail:
                    foreach (var item in errTemplates)
                    {
                        if (ManageValidationRule(obj, item))
                        {
                            err = item;
                            break;
                        }
                    }
                    break;
                case ValidationLogic.BreakAtFirstFailButRunAllWarnings:

                    foreach (var warning in errTemplates.Where(s => s is Warning<T>))
                    {
                        ManageValidationRule(obj, warning);
                    }

                    foreach (var item in errTemplates.Where(s => s is Error<T>))
                    {
                        var isFailed = ManageValidationRule(obj, item);
                        if (isFailed)
                        {
                            break;
                        }
                    }
                    break;
            }

            if (!refference.ToArray().Any(s => s is Error<T> || s is Warning<T>))
            {
                Error = string.Empty;
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
                ErrorInfoProviderSimpleAccessAdapter.Errors.Add(item);
                Error = string.Format(MessageFormat, item.ErrorType, item.ErrorText);
            }

            // Bedingung ist falsch und error ist in der liste der angezeigten errors
            if (!conditionresult && ErrorInfoProviderSimpleAccessAdapter.Errors.Contains(item))
                ErrorInfoProviderSimpleAccessAdapter.Errors.Remove(item);
            else if (ErrorInfoProviderSimpleAccessAdapter.Errors.Contains(item))
                Error = item.ErrorText;

            return conditionresult;
        }
    }
}