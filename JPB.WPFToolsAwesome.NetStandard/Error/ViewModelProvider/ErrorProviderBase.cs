using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Serialization;
using JPB.Tasking.TaskManagement;
using JPB.WPFToolsAwesome.Error;
using JPB.WPFToolsAwesome.Error.ValidationTypes;
using JPB.WPFToolsAwesome.MVVM.ViewModel;

namespace JPB.ErrorValidation.ViewModelProvider
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
		///     Check failed Errors on next Invoke even if they are not explicit called.
		/// </summary>
		[Browsable(false)]
		public bool AlwaysCheckFailedInNextRun { get; set; }

		/// <inheritdoc />
		public virtual async Task ReplaceUserErrorCollection(IErrorCollectionBase userErrors)
		{
			ActiveValidationCases.Clear();
			_userErrors = userErrors;
			await Task.CompletedTask;
		}
		
		/// <inheritdoc />
		public virtual IErrorCollectionBase UserErrors
		{
			get { return _userErrors; }
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
		public virtual async Task ForceRefreshAsync()
		{
			if (Validate)
			{
				foreach (var error in UserErrors.SelectMany(s => s.ErrorIndicator).Distinct())
				{
					await ValidateObject(error, this);
				}
			}
		}

		/// <summary>
		///     Gets all Errors for the Field
		/// </summary>
		/// <param name="fieldName"></param>
		/// <param name="validationObject"></param>
		/// <returns></returns>
		[Browsable(false)]
		public IValidation[] GetError(string fieldName, object validationObject)
		{
			return AsyncHelper.WaitSingle(ValidateObject(fieldName, validationObject));
		}

		private void InitErrorProvider(IErrorCollectionBase errors)
		{
			ViewModelAction(() =>
			{
				ActiveValidationCases = new ThreadSaveObservableCollection<IValidation>();
				_userErrors = errors;
				PropertyChanged += ErrorProviderBase_PropertyChanged;
				Validation = ValidationLogic.RunThroughAll;
				Validate = true;
				AlwaysCheckFailedInNextRun = true;
			});
		}

		protected virtual async void ErrorProviderBase_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			await HandleUnboundProperties(e.PropertyName);
		}

		protected virtual async Task HandleUnboundProperties(string fieldName)
		{
			await ObManage(
				UserErrors.Where(f =>
					f.Unbound && (string.IsNullOrWhiteSpace(fieldName) || f.ErrorIndicator.Contains(fieldName))), this);
		}

		/// <summary>
		///     Overwrite to create your own Validation logic
		/// </summary>
		/// <param name="errorsForField"></param>
		/// <returns></returns>
		[Browsable(false)]
		protected virtual IEnumerable<IValidation> ValidateErrors(IEnumerable<IValidation> errorsForField)
		{
			throw new NotImplementedException("You have enabled ValidateSelf but did not overwrite the 'ValidateErrors' method");
		}

		/// <summary>
		///     The main function for Starting an Validation Cycle
		/// </summary>
		/// <param name="fieldName"></param>
		/// <param name="validationObject"></param>
		/// <returns></returns>
		protected Task<IValidation[]> ValidateObject(string fieldName, object validationObject)
		{
			return ObManage(ProduceValidations(fieldName), validationObject);
		}

		/// <summary>
		///     Returns all known Errors of this Instance
		/// </summary>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		protected virtual IEnumerable<IValidation> ProduceValidations(string fieldName)
		{
			return UserErrors.FilterErrors(fieldName);
		}

		/// <summary>
		///     The main Validation logic that handels all errors base on the current Validation flag
		/// </summary>
		/// <param name="errorsForField"></param>
		/// <param name="validationObject"></param>
		/// <returns></returns>
		protected async Task<IValidation[]> ObManage(IEnumerable<IValidation> errorsForField, object validationObject)
		{
			if (!Validate)
			{
				if (ActiveValidationCases.Any())
				{
					ActiveValidationCases.Clear();
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
						if (await ManageValidationRule(validationObject, error))
						{
							errorsOfThisRun.Add(error);
						}
					}
					break;
				case ValidationLogic.BreakAtFirstFail:
					foreach (var item in forField)
					{
						if (await ManageValidationRule(validationObject, item))
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
					if (await ManageValidationRule(validationObject, error))
					{
						errorsOfThisRun.Add(error);
					}
					else
					{
						errorsOfThisRun.Remove(error);
					}
				}
			}

			return errorsOfThisRun.ToArray();
		}

		protected async Task<bool> ManageValidationRule(object validationObject, IValidation validation)
		{
			bool isError;
			try
			{
				if (validation is IAsyncValidation asyncValidation)
				{
					var asyncValidationAsyncCondition = asyncValidation.AsyncCondition(validationObject);
					isError = await asyncValidationAsyncCondition || asyncValidationAsyncCondition.IsFaulted;

				}
				else
				{
					isError = validation.Condition(validationObject);
				}
			}
			catch (Exception)
			{
				isError = true;
			}

			//Condition is true and error is in our list of errors
			if (isError && !ActiveValidationCases.Contains(validation))
			{
				ActiveValidationCases.Add(validation);
				SendPropertyChanged(() => HasError);
				return true;
			}

			//Error is known
			if (ActiveValidationCases.Contains(validation))
			{
				if (!isError)
				{
					ActiveValidationCases.Remove(validation);
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
		}
	}
}