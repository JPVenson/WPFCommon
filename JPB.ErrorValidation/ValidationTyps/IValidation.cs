using System;

namespace JPB.ErrorValidation.ValidationTyps
{
	public interface IValidation
	{
		/// <summary>
		///		An description what kind of Validation is used e.g "Warning", "Error", "FYI"
		/// </summary>
		[Obsolete("This Property is Obsolete. If you need this functionality please build it yourself by packing it into the ErrorText")] 
		object ErrorType { get; }

		/// <summary>
		///		An indicator for all fields that participate on the Validation
		/// </summary>
		string[] ErrorIndicator { get; set; }

		/// <summary>
		///		The text that should be emitted when the Condition is True
		/// </summary>
		object ErrorText { get; set; }

		/// <summary>
		///		The Condition that indicates an Error. True error, False NoError
		/// </summary>
		Func<object, bool> Condition { get; set; }

		/// <summary>
		///		Set this to true to simulate a Virtual call the the underlying Error provider.
		///		Unbound Properties are only executed when Explicitly called by the INotifyPropertyChanged event
		/// </summary>
		bool Unbound { get; set; }
	}

	public interface IValidation<T> : IValidation
	{
		/// <summary>
		/// The Condition that indicates an Error. True error, False NoError
		/// </summary>
		new Func<T, bool> Condition { get; set; }
	}
}