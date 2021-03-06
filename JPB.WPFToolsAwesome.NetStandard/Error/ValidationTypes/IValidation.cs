﻿using System;

namespace JPB.WPFToolsAwesome.Error.ValidationTypes
{
	public interface IValidation
	{
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
		bool Unbound { get; }
	}

	public interface IValidation<T> : IValidation
	{
		/// <summary>
		/// The Condition that indicates an Error. True error, False NoError
		/// </summary>
		new Func<T, bool> Condition { get; set; }
	}
}