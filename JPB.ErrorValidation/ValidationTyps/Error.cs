using System;
using System.Linq;
using System.Linq.Expressions;

namespace JPB.ErrorValidation.ValidationTyps
{
	/// <summary>
	///     Defines an static Error that will be displayed when the condition is met
	/// </summary>
	public class Error<T> : IValidation<T>
	{
		private Func<T, bool> _condition;
		private string[] _errorIndicator;
		private object _errorText;

		/// <summary>
		///		Creates a new Error object that defines a single validation that can be proved
		/// </summary>
		/// <param name="errorText">The Error object. This object is given to the UI to display this error.</param>
		/// <param name="errorIndicator">Defines a single or Multiple Properties on the Target ViewModel that this validation relies on. This defines when the Validation will be executed as well as where it will be displayed.</param>
		/// <param name="condition">The Condition on which this error defines as "Failed" or "Success"</param>
		public Error(object errorText, string errorIndicator, Func<T, bool> condition) : this(errorText, condition,
			errorIndicator)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="errorText">The Error object. This object is given to the UI to display this error.</param>
		/// <param name="errorIndicator">Defines a single or Multiple Properties on the Target ViewModel that this validation relies on. This defines when the Validation will be executed as well as where it will be displayed.</param>
		/// <param name="condition">The Condition on which this error defines as "Failed" or "Success"</param>
		public Error(object errorText, Func<T, bool> condition, params string[] errorIndicator)
		{
			_condition = condition;
			_errorIndicator = errorIndicator;
			_errorText = errorText;
		}

		public static Error<T> FromProperty<TProperty>(object errorText, Func<T, bool> condition,
			Expression<Func<TProperty>> errorIndicator)
		{
			return new Error<T>(errorText, condition, ViewModelBase.GetPropertyName(errorIndicator));
		}

		public Error<T> Include<TProperty>(Expression<Func<TProperty>> errorIndicator)
		{
			ErrorIndicator = ErrorIndicator.Concat(new[] {ViewModelBase.GetPropertyName(errorIndicator)}).ToArray();
			return this;
		}

		#region IValidation<T> Members
		
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
			set { _errorText = value; }
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

		#endregion
	}

	
	/// <summary>
	///     Defines an Error
	/// </summary>
	public class Error : IValidation
	{
		private Func<object, bool> _condition;
		private string[] _errorIndicator;
		private object _errorText;

		/// <summary>
		///		Creates a new Error object that defines a single validation that can be proved
		/// </summary>
		/// <param name="errorText">The Error object. This object is given to the UI to display this error.</param>
		/// <param name="errorIndicator">Defines a single or Multiple Properties on the Target ViewModel that this validation relies on. This defines when the Validation will be executed as well as where it will be displayed.</param>
		/// <param name="condition">The Condition on which this error defines as "Failed" or "Success"</param>
		public Error(object errorText, string errorIndicator, Func<object, bool> condition) : this(errorText, condition,
			errorIndicator)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="errorText">The Error object. This object is given to the UI to display this error.</param>
		/// <param name="errorIndicator">Defines a single or Multiple Properties on the Target ViewModel that this validation relies on. This defines when the Validation will be executed as well as where it will be displayed.</param>
		/// <param name="condition">The Condition on which this error defines as "Failed" or "Success"</param>
		public Error(object errorText, Func<object, bool> condition, params string[] errorIndicator)
		{
			_condition = condition;
			_errorIndicator = errorIndicator;
			_errorText = errorText;
		}
		

		#region IValidation<T> Members
		
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
			set { _errorText = value; }
		}
		
		/// <inheritdoc />
		public virtual Func<object, bool> Condition
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
		public bool Unbound { get; set; }

		#endregion
	}
}