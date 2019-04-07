using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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
		public enum ValidationLogic
		{
			/// <summary>
			///		The Error evaluation will stop when it first encounters an error.
			/// </summary>
			BreakAtFirstFail,

			/// <summary>
			///		All affected errors will be validated
			/// </summary>
			RunThroughAll,

			/// <summary>
			///     Calls the <see cref="ErrorProviderBase.ValidateErrors"/> 
			/// </summary>
			ValidateSelf
		}

		private string _error;
		private IErrorCollectionBase _userErrors;

		protected ErrorProviderBase(Dispatcher dispatcher, IErrorCollectionBase errors)
			: base(dispatcher)
		{
			InitErrorProvider(errors);
		}

		protected ErrorProviderBase(IErrorCollectionBase errors)
		{
			InitErrorProvider(errors);
		}

		/// <summary>
		///     How should be validated
		/// </summary>
		[Browsable(false)]
		[XmlIgnore]
		protected ValidationLogic Validation { get; set; }

		/// <summary>
		///     For IDataErrorInfo support with multiple Errors for one field.
		/// </summary>
		[Browsable(false)]
		[XmlIgnore]
		public Func<object, object, object> AggregateMultiError { get; set; }

		/// <summary>
		///     Check failed Errors on next Invoke even if they are not explicit called.
		/// </summary>
		[Browsable(false)]
		public bool AlwaysCheckFailedInNextRun { get; set; }

		///<inheritdoc />
		public string Error
		{
			get
			{
				if (Validate)
				{
					return _error;
				}

				return string.Empty;
			}
			set
			{
				if (value == _error)
				{
					return;
				}

				ThreadSaveAction(() =>
				{
					_error = value;
					SendPropertyChanged();
				});
			}
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

		/// <inheritdoc />
		[XmlIgnore]
		public ICollection<IValidation> ActiveValidationCases { get; set; }
		
		/// <inheritdoc />
		[XmlIgnore]
		public bool HasError
		{
			get { return ActiveValidationCases.Any(); }
		}

		/// <inheritdoc />
		[Browsable(false)]
		public string MessageFormat { get; set; }


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
		///     Gets all Errors for the Field
		/// </summary>
		/// <param name="columnName"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		[Browsable(false)]
		public IValidation[] GetError(string columnName, object obj)
		{
			return ObManage(columnName, obj);
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
			HandleUnboundPropertys(e.PropertyName);
		}

		private void HandleUnboundPropertys(string propName)
		{
			ObManage(
				UserErrors.Where(f =>
					f.Unbound && (string.IsNullOrWhiteSpace(propName) || f.ErrorIndicator.Contains(propName))), this);
		}

		/// <summary>
		///     Overwrite to create your own Validation logic
		/// </summary>
		/// <param name="errorsForField"></param>
		/// <returns></returns>
		[Browsable(false)]
		protected virtual IEnumerable<IValidation> ValidateErrors(IEnumerable<IValidation> errorsForField)
		{
			throw new NotImplementedException("Please create your own Validation");
		}

		/// <summary>
		///     The main function for Starting an Validation Cycle
		/// </summary>
		/// <param name="errorIndicator"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		protected IValidation[] ObManage(string errorIndicator, object obj)
		{
			return ObManage(ProduceValidations(errorIndicator), obj);
		}

		/// <summary>
		///     Returns all known Errors of this Instance
		/// </summary>
		/// <param name="errorIndicator"></param>
		/// <returns></returns>
		protected virtual IEnumerable<IValidation> ProduceValidations(string errorIndicator)
		{
			return UserErrors.ReturnErrors(errorIndicator);
		}

		/// <summary>
		///     The main Validation logic that handels all errors base on the current Validation flag
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
					foreach (var validateError in ValidateErrors(forField))
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
					Error = errorsOfThisRun
						.Select(e => e.ErrorText)
						.Aggregate(
						(current, validation) => AggregateMultiError(current, validation))
						?.ToString();
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
			bool isError;
			try
			{
				isError = item.Condition(obj);
			}
			catch (Exception)
			{
				isError = true;
			}

			//Condition is true and error is in our list of errors
			if (isError && !ActiveValidationCases.Contains(item))
			{
				ActiveValidationCases.Add(item);
				SendPropertyChanged(() => HasError);
				return true;
			}

			//Error is Kown
			if (ActiveValidationCases.Contains(item))
			{
				if (!isError)
				{
					ActiveValidationCases.Remove(item);
					SendPropertyChanged(() => HasError);
				}
				else
				{
					return true;
				}
			}

			return isError;
		}

		/// <summary>
		///     Will trigger when the list of Errors has Changed
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged
		{
			add
			{
				if (ActiveValidationCases is INotifyCollectionChanged changed)
				{
					changed.CollectionChanged += value;
				}
			}
			remove
			{
				if (ActiveValidationCases is INotifyCollectionChanged changed)
				{
					changed.CollectionChanged -= value;
				}
			}
		}
	}

	public abstract class ErrorProviderBase<T> : ErrorProviderBase where T : class, IErrorCollectionBase, new()
	{
		protected ErrorProviderBase(Dispatcher dispatcher) : base(dispatcher, new T())
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