using System;

namespace JPB.WPFToolsAwesome.MVVM.ViewModel
{
	/// <summary>
	///     Eventargs for Accepting or Cancel a change on a Property
	/// </summary>
	public class AcceptPendingChangeEventArgs : EventArgs
	{
		/// <summary>
		///     ctor
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="newValue"></param>
		public AcceptPendingChangeEventArgs(string propertyName, object newValue, object oldValue)
		{
			PropertyName = propertyName;
			NewValue = newValue;
			OldValue = oldValue;
		}

		/// <summary>
		///     The Name of the Property that should be changed
		/// </summary>
		public string PropertyName { get; }

		/// <summary>
		///     The value to be applied to the property
		/// </summary>
		public object NewValue { get; }

		/// <summary>
		///		The old value of the property
		/// </summary>
		public object OldValue { get; }

		/// <summary>
		///     If set to true inside the event handler, the value will be not applied to the property
		/// </summary>
		public bool CancelPendingChange { get; set; }

		// flesh this puppy out
	}
}