using System;
using System.Linq;
using System.Linq.Expressions;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.ErrorValidation.ValidationTyps
{
	/// <summary>
	///     Defines an Error
	/// </summary>
	public class Error<T> : IValidation<T>
	{
		private string[] _errorIndicator;
		private string _errorText;
		private Func<T, bool> _condition;

		public Error(string errorText, string errorIndicator, Func<T, bool> condition) : this(errorText, condition, errorIndicator)
		{

		}

		public Error(string errorText, Func<T, bool> condition, params string[] errorIndicator)
		{
			this._condition = condition;
			this._errorIndicator = errorIndicator;
			this._errorText = errorText;
		}

		public static Error<T> FromProperty<TProperty>(string errorText, Func<T, bool> condition,
			Expression<Func<TProperty>> errorIndicator)
		{
			return new Error<T>(errorText, condition, ViewModelBase.GetPropertyName(errorIndicator));
		}

		public Error<T> Include<TProperty>(Expression<Func<TProperty>> errorIndicator)
		{
			ErrorIndicator = ErrorIndicator.Concat(new[] { ViewModelBase.GetPropertyName(errorIndicator) }).ToArray();
			return this;
		}

		#region IValidation<T> Members

		public virtual string[] ErrorIndicator
		{
			get { return _errorIndicator; }
			set { _errorIndicator = value; }
		}

		public virtual string ErrorText
		{
			get { return _errorText; }
			set { _errorText = value; }
		}

		public virtual Func<T, bool> Condition
		{
			get { return _condition; }
			set { _condition = value; }
		}

		public virtual string ErrorType
		{
			get { return "Need"; }
		}

		/// <summary>
		/// The Condition that indicates an Error. True error, False NoError
		/// </summary>
		Func<object, bool> IValidation.Condition
		{
			get
			{
				return (validator) =>
				{
					if (!(validator is T))
						return true;
					return Condition((T)validator);
				};
			}
			set
			{
				Condition = (arg) => value(arg);
			}
		}

		public bool Unbound { get; set; }

		#endregion
	}

	public class AsyncError<T> : Error<T>, IAsyncValidation
	{
		public AsyncError(string errorText, string errorIndicator, Func<T, bool> condition) : this(errorText, condition, errorIndicator)
		{
		}

		public AsyncError(string errorText, Func<T, bool> condition, params string[] errorIndicator)
			: this(errorText, condition, AsyncState.AsyncSharedPerCall, AsyncRunState.CurrentPlusOne, errorIndicator)
		{
		}

		public AsyncError(string errorText, Func<T, bool> condition, AsyncState state, AsyncRunState runState, params string[] errorIndicator)
			: base(errorText, condition, errorIndicator)
		{
			AsyncState = state;
			RunState = runState;
		}

		public AsyncState AsyncState { get; set; }
		public AsyncRunState RunState { get; set; }
	}
}