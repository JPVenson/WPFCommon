using System;

namespace JPB.ErrorValidation.ValidationTyps
{
	/// <summary>
	///		Allows the scheduling of an async Validation. 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class AsyncError<T> : Error<T>, IAsyncValidation
	{
		public AsyncError(string errorText, string errorIndicator, Func<T, bool> condition) : this(errorText, condition,
			errorIndicator)
		{
		}

		public AsyncError(string errorText, Func<T, bool> condition, params string[] errorIndicator)
			: this(errorText, condition, AsyncState.AsyncSharedPerCall, AsyncRunState.CurrentPlusOne, errorIndicator)
		{
		}

		public AsyncError(string errorText, Func<T, bool> condition, AsyncState state, AsyncRunState runState,
			params string[] errorIndicator)
			: base(errorText, condition, errorIndicator)
		{
			AsyncState = state;
			RunState = runState;
		}

		public AsyncState AsyncState { get; set; }
		public AsyncRunState RunState { get; set; }
	}
}