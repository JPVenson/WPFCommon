using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Threading;
using System.Xml.Serialization;
using JPB.ErrorValidation.ValidationTyps;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.ErrorValidation.ViewModelProvider.Base
{
	public abstract class ErrorProviderBase :
		AsyncViewModelBase,
		IErrorValidatorBase
	{
		private string _error;
		private IErrorCollectionBase _userErrors;
        ///<inheritdoc />
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

				base.ThreadSaveAction(() =>
				{
					_error = value;
					SendPropertyChanged();
				});
			}
		}

		protected ErrorProviderBase(Dispatcher disp, IErrorCollectionBase errors)
			: base(disp)
		{
			this.InitErrorProvider(errors);
		}

		protected ErrorProviderBase(IErrorCollectionBase errors)
		{
			this.InitErrorProvider(errors);
		}

		private void InitErrorProvider(IErrorCollectionBase errors)
		{
			ActiveValidationCases = new ThreadSaveObservableCollection<IValidation>();
		   

            UserErrors = errors;
			PropertyChanged += ErrorProviderBase_PropertyChanged;
			MessageFormat = "{1}";
			Validation = ValidationLogic.RunThroughAll;
			Validate = true;
			AlwaysCheckFailedInNextRun = true;
		}

		private void ErrorProviderBase_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.HandleUnboundPropertys(e.PropertyName);
		}

		private void HandleUnboundPropertys(string propName)
		{
			ObManage(UserErrors.Where(f => f.Unbound && (string.IsNullOrWhiteSpace(propName) || f.ErrorIndicator.Contains(propName))), this);
		}

        /// <inheritdoc />
        public virtual IErrorCollectionBase UserErrors
		{
			get { return _userErrors; }
			set
			{
				ActiveValidationCases.Clear();
				_userErrors = value;
				ForceRefresh();
			}
		}
        
        /// <inheritdoc />
        [Browsable(false)]
		public bool Validate { get; set; }

		/// <summary>
		/// How should be validated
		/// </summary>
		[Browsable(false)]
		[XmlIgnore]
		protected ValidationLogic Validation { get; set; }

        /// <inheritdoc />
        [XmlIgnore]
        public ICollection<IValidation> ActiveValidationCases { get; set; }

        /// <summary>
        /// The list of all Active Errors
        /// </summary>
        //[XmlIgnore]
        //public ICollectionView ActiveValidationCasesView { get; set; }


        /// <inheritdoc />
        [XmlIgnore]
		public bool HasError
		{
			get { return ActiveValidationCases.Any(); }
		}

        /// <inheritdoc />
        [Browsable(false)]
		public string MessageFormat { get; set; }

		/// <summary>
		/// For IDataErrorInfo support with multibe Errors for one field.
		/// </summary>
		[Browsable(false)]
		[XmlIgnore]
		public Func<string, string, string> AggregateMultiError { get; set; }

		/// <summary>
		/// Check failed Errors on next Invoke
		/// </summary>
		[Browsable(false)]
		public bool AlwaysCheckFailedInNextRun { get; set; }

		public enum ValidationLogic
		{
			BreakAtFirstFail,
            [Obsolete("The concept of Warnings is Obsolete and will be removed")]
			BreakAtFirstFailButRunAllWarnings,
			RunThroughAll,
            /// <summary>
            /// Do not use a specifc logic but use the ValidateErrors function to enumerate the Errors
            /// </summary>
			ValidateSelf,
		}


	    /// <inheritdoc />
	    [Browsable(false)]
		public virtual void ForceRefresh()
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
		[Browsable(false)]
		public IValidation[] GetError(string columnName, object obj)
		{
			return ObManage(columnName, obj);
		}

		/// <summary>
		/// Overwrite to create your own Validation logic
		/// </summary>
		/// <param name="errorsForField"></param>
		/// <returns></returns>
		[Browsable(false)]
		protected virtual IEnumerable<IValidation> ValidateErrors(IEnumerable<IValidation> errorsForField)
		{
			throw new NotImplementedException("Please create your own Validation");
		}

        /// <summary>
        /// The main function for Starting an Validation Cycle
        /// </summary>
        /// <param name="errorIndicator"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
		protected IValidation[] ObManage(string errorIndicator, object obj)
		{
		   return ObManage(ProduceValidations(errorIndicator), obj);
		}

		/// <summary>
		/// Returns all known Errors of this Instance
		/// </summary>
		/// <param name="errorIndicator"></param>
		/// <returns></returns>
		protected virtual IEnumerable<IValidation> ProduceValidations(string errorIndicator)
		{
			return UserErrors.ReturnErrors(errorIndicator);
		}

        /// <summary>
        /// The main Validation logic that handels all errors base on the current Validation flag
        /// </summary>
        /// <param name="errorsForField"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
		protected IValidation[] ObManage(IEnumerable<IValidation> errorsForField, object obj)
		{
			if (!Validate)
			{
			    if (ActiveValidationCases.Any())
			    {
			        ActiveValidationCases.Clear();
                    Error = string.Empty;
                    SendPropertyChanged(() => HasError);
                }
				return new IValidation[0];
			}

			var errorsOfThisRun = new HashSet<IValidation>();

		    var forField = errorsForField as IValidation[] ?? errorsForField.ToArray();
		    switch (Validation)
			{
				case ValidationLogic.RunThroughAll:
					foreach (var error in forField)
					{
						if (ManageValidationRule(obj, error))
						{
							errorsOfThisRun.Add(error);
						}
					}
					break;
				case ValidationLogic.BreakAtFirstFail:
					foreach (var item in forField)
					{
						if (ManageValidationRule(obj, item))
						{
							errorsOfThisRun.Add(item);
							break;
						}
					}
					break;
				case ValidationLogic.ValidateSelf:
					foreach (var validateError in this.ValidateErrors(forField))
					{
						errorsOfThisRun.Add(validateError);
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (AlwaysCheckFailedInNextRun)
			{
				foreach (var error in forField.Except(errorsOfThisRun).Where(s => ActiveValidationCases.Contains(s)))
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

			if (!ActiveValidationCases.Any())
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

		protected bool ManageValidationRule(object obj, IValidation item)
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
				SendPropertyChanged(() => HasError);
				return true;
			}
			//Error is Kown
			if (ActiveValidationCases.Contains(item))
			{
				if (!conditionresult)
				{
					ActiveValidationCases.Remove(item);
					SendPropertyChanged(() => HasError);
				}
				else
				{
					return true;
				}
			}

			return conditionresult;
		}

		/// <summary>
		/// Will trigger when the list of Errors has Changed
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged
		{
			add
			{
				var changed = ActiveValidationCases as INotifyCollectionChanged;
				if (changed != null)
				{
					changed.CollectionChanged += value;
				}
			}
			remove
			{
				var changed = ActiveValidationCases as INotifyCollectionChanged;
				if (changed != null)
				{
					changed.CollectionChanged -= value;
				}
			}
		}

		[Browsable(false)]
        [Obsolete("The concept of Warnings is Obsolete and will be removed")]
        public bool WarningAsFailure { get; set; }

		public virtual Type RetrunT()
		{
			return GetType();
		}
	}

	public abstract class ErrorProviderBase<T> : ErrorProviderBase where T : class, IErrorCollectionBase, new()
	{
		protected ErrorProviderBase(Dispatcher disp) : base(disp, new T())
		{
		}

		protected ErrorProviderBase() : base(new T())
		{
		}

		public new T UserErrors
		{
			get { return base.UserErrors as T; }
			set { base.UserErrors = value; }
		}
	}
}