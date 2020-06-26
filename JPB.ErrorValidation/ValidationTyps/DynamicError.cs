using System;
using JetBrains.Annotations;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.ErrorValidation.ValidationTyps
{
	/// <summary>
	///		Defines an Error that will be displayed when the condition is met. Allows to update the ErrorText when the error changes
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class DynamicError<T> : ViewModelBase, IValidation<T>
	{
		private Func<T, bool> _condition;
		private string[] _errorIndicator;
		private object _errorText;

		public class ErrorConditionModel
		{
			public ErrorConditionModel(T viewModel)
			{
				ViewModel = viewModel;
			}

			public T ViewModel { get; }

			public object ErrorText { get; set; }
			public bool IsError { get; set; }
		}

		/// <summary>
		///		Creates a new Error object that defines a single validation that can be proved
		/// </summary>
		/// <param name="errorText">The Error object. This object is given to the UI to display this error.</param>
		/// <param name="errorIndicator">Defines a single or Multiple Properties on the Target ViewModel that this validation relies on. This defines when the Validation will be executed as well as where it will be displayed.</param>
		/// <param name="condition">The Condition on which this error defines as "Failed" or "Success"</param>
		public DynamicError([CanBeNull] object errorText,
			[CanBeNull] string errorIndicator,
			[NotNull] Action<ErrorConditionModel> condition) 
			: this(errorText, condition, errorIndicator)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="errorText">The Error object. This object is given to the UI to display this error.</param>
		/// <param name="errorIndicator">Defines a single or Multiple Properties on the Target ViewModel that this validation relies on. This defines when the Validation will be executed as well as where it will be displayed.</param>
		/// <param name="condition">The Condition on which this error defines as "Failed" or "Success"</param>
		public DynamicError([CanBeNull] object errorText, 
			[NotNull] Action<ErrorConditionModel> condition, 
			[NotNull] params string[] errorIndicator)
		{
			_errorIndicator = errorIndicator;
			_errorText = errorText;

			_condition = (vm) =>
			{
				var model = new ErrorConditionModel(vm);
				model.ErrorText = errorText;
				condition(model);
				errorText = model.ErrorText;
				return model.IsError;
			};
		}
		
		/// <inheritdoc />
		public virtual string[] ErrorIndicator
		{
			get { return _errorIndicator; }
			set { _errorIndicator = value; }
		}
		
		/// <inheritdoc />
		public virtual object ErrorText
		{
			get { return _errorText; }
			set
			{
				SendPropertyChanging();
				_errorText = value;
				SendPropertyChanged();
			}
		}
		
		/// <inheritdoc />
		public virtual Func<T, bool> Condition
		{
			get { return _condition; }
			set { _condition = value; }
		}

		/// <inheritdoc />
		public virtual object ErrorType
		{
			get { return "Need"; }
		}

		/// <inheritdoc />
		Func<object, bool> IValidation.Condition
		{
			get
			{
				return validator =>
				{
					if (!(validator is T))
					{
						return true;
					}

					return Condition((T) validator);
				};
			}
			set { Condition = arg => value(arg); }
		}

		/// <inheritdoc />
		public bool Unbound { get; set; }
	}
}