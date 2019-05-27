namespace JPB.WPFBase.MVVM.ViewModel
{
	/// <summary>
	///     Handling Interface for Cancel changes. Must be used with <see cref="ViewModelBase.SetProperty{TArgument}(ref TArgument,TArgument,string)"/> in the ViewModel
	/// </summary>
	public interface IAcceptPendingChange
	{
		/// <summary>
		///     Can be used to cancel an value change
		/// </summary>
		event AcceptPendingChangeHandler PendingChange;
	}
}