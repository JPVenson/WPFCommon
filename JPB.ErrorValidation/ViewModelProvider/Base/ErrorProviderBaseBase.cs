using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Threading;
using System.Xml.Serialization;
using JPB.ErrorValidation.ValidationTyps;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.ErrorValidation.ViewModelProvider.Base
{
    public abstract class ErrorProviderBase<T> :
        AsyncViewModelBase,
        IErrorValidatorBase<T>
        where T : class, IErrorCollectionBase, new()
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
                    SendPropertyChanged();
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
            UserErrors = new T();
            ActiveValidationCases = new ObservableCollection<IValidation>();
            MessageFormat = "{0} - {1}";
            Validation = ValidationLogic.RunThroughAll;
            Validate = true;
            AlwaysCheckFailedInNextRun = true;
        }

        public T UserErrors { get; set; }

        public bool Validate { get; set; }

        [XmlIgnore]
        protected ValidationLogic Validation { get; set; }

        [XmlIgnore]
        public ICollection<IValidation> ActiveValidationCases { get; set; }

        [XmlIgnore]
        public bool HasError
        {
            get { return ActiveValidationCases.Any(); }
        }

        public string MessageFormat { get; set; }

        [XmlIgnore]
        public Func<string, string, string> AggregateMultiError { get; set; }

        public bool AlwaysCheckFailedInNextRun { get; set; }

        public enum ValidationLogic
        {
            BreakAtFirstFail,
            BreakAtFirstFailButRunAllWarnings,
            RunThroughAll,
            ValidateSelf,
        }

        /// <summary>
        /// Refresh the Errors
        /// </summary>
        public void ForceRefresh()
        {
            if (Validate)
            {
                foreach (var error in UserErrors.SelectMany(s => s.ErrorIndicator).Distinct())
                {
                    ObManage(error, this);
                }
            }
        }

        /// <summary>
        /// Gets all Errors for the Field
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public IValidation[] GetError(string columnName, object obj)
        {
            return ObManage(columnName, obj);
        }

        /// <summary>
        /// Overwrite to create your own Validation logic
        /// </summary>
        /// <param name="errorsForField"></param>
        /// <returns></returns>
        protected virtual IEnumerable<IValidation> ValidateErrors(IEnumerable<IValidation> errorsForField)
        {
            throw new NotImplementedException("Please create your Owin Validation");
        }

        private IValidation[] ObManage(string errorIndicator, object obj)
        {
            var errorsOfThisRun = new HashSet<IValidation>();

            if (!Validate)
            {
                if (ActiveValidationCases.Any())
                    ActiveValidationCases.Clear();
                Error = string.Empty;
                return errorsOfThisRun.ToArray();
            }

            var errors = ActiveValidationCases;

            var errorsForField =
                UserErrors
                    .Where(s => s.ErrorIndicator.Contains(errorIndicator) || !s.ErrorIndicator.Any());

            switch (Validation)
            {
                case ValidationLogic.RunThroughAll:
                    foreach (var error in errorsForField)
                    {
                        if (ManageValidationRule(obj, error))
                        {
                            errorsOfThisRun.Add(error);
                        }
                    }
                    break;
                case ValidationLogic.BreakAtFirstFail:
                    foreach (var item in errorsForField)
                    {
                        if (ManageValidationRule(obj, item))
                        {
                            errorsOfThisRun.Add(item);
                            break;
                        }
                    }
                    break;
                case ValidationLogic.ValidateSelf:
                    foreach (var validateError in this.ValidateErrors(errorsForField))
                    {
                        errorsOfThisRun.Add(validateError);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (AlwaysCheckFailedInNextRun)
            {
                foreach (var error in errorsForField.Except(errorsOfThisRun).Where(s => errors.Contains(s)))
                {
                    if (ManageValidationRule(obj, error))
                    {
                        errorsOfThisRun.Add(error);
                    }
                    else
                    {
                        errorsOfThisRun.Remove(error);
                    }
                }
            }

            if (!errors.Any())
            {
                Error = string.Empty;
            }
            else
            {
                if (AggregateMultiError != null)
                {
                    Error = errorsOfThisRun.Aggregate("",
                        (current, validation) => AggregateMultiError(current, validation.ErrorText));
                }
                else
                {
                    var fod = errorsOfThisRun.FirstOrDefault();
                    if (fod != null && fod.ErrorText != Error)
                    {
                        Error = string.Format(MessageFormat, fod.ErrorType, fod.ErrorText);
                    }
                }
            }
            return errorsOfThisRun.ToArray();
        }

        private bool ManageValidationRule(object obj, IValidation item)
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

            //Condition is true and error is in our list of errors
            if (conditionresult && !ActiveValidationCases.Contains(item))
            {
                ActiveValidationCases.Add(item);
                return true;
            }
            //Error is Kown
            if (ActiveValidationCases.Contains(item))
            {
                if (!conditionresult)
                {
                    ActiveValidationCases.Remove(item);
                }
                else
                {
                    return true;
                }
            }

            return conditionresult;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public bool WarningAsFailure { get; set; }

        public virtual Type RetrunT()
        {
            return GetType();
        }
    }
}