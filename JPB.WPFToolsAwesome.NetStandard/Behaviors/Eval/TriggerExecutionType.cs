using System.ComponentModel;

namespace JPB.WPFToolsAwesome.Behaviors.Eval
{
	/// <summary>
	///		Defines the types for when the evaulation of the triggers will respond to an changing property
	/// </summary>
	public enum TriggerExecutionType
	{
		/// <summary>
		///		Executes the Trigger directly after <see cref="INotifyPropertyChanged.PropertyChanged"/> from an Trigger was executed
		/// </summary>
		Immediate,

		/// <summary>
		///		Executes the Trigger later after <see cref="INotifyPropertyChanged.PropertyChanged"/> was called
		/// </summary>
		Later
	}
}