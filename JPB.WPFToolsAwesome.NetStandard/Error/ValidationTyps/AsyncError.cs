using System;
using System.Threading.Tasks;

namespace JPB.WPFToolsAwesome.Error.ValidationTyps
{
	/// <summary>
	///		Allows the scheduling of an async Validation. 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class AsyncError<T> : Error<T>, IAsyncValidation<T>
	{
		/// <inheritdoc />
		public AsyncError(string errorText, string errorIndicator, Func<T, Task<bool>> condition) : this(errorText, condition,
			errorIndicator)
		{
		}

		/// <inheritdoc />
		public AsyncError(string errorText, Func<T, Task<bool>> condition, params string[] errorIndicator)
			: this(errorText, condition, AsyncState.AsyncSharedPerCall, AsyncRunState.CurrentPlusOne, errorIndicator)
		{
		}

		/// <inheritdoc />
		public AsyncError(string errorText, Func<T, Task<bool>> condition, AsyncState state, AsyncRunState runState,
			params string[] errorIndicator)
			: base(errorText, null, errorIndicator)
		{
			AsyncState = state;
			RunState = runState;
			AsyncCondition = condition;
		}

		/// <inheritdoc />
		public override Func<T, bool> Condition
		{
			get
			{
				throw new InvalidOperationException("This is an AsyncError that has an AsyncCondition. It cannot be executed synchronized.");
			}
			set
			{
				if (value != null)
				{
					throw new InvalidOperationException(
						"This is an AsyncError that has an AsyncCondition. It cannot be executed synchronized.");
				}
			}
		}

		/// <inheritdoc />
		public AsyncState AsyncState { get; set; }
		/// <inheritdoc />
		public AsyncRunState RunState { get; set; }

		Func<object, Task<bool>> IAsyncValidation.AsyncCondition
		{
			get
			{
				return async validator =>
				{
					if (!(validator is T))
					{
						return false;
					}

					return await AsyncCondition((T)validator);
				};
			}
			set { AsyncCondition = arg => value(arg); }
		}

		/// <inheritdoc />
		public Func<T, Task<bool>> AsyncCondition
		{
			get;
			set;
		}
	}
}