namespace JPB.WPFBase.MVVM.ViewModel
{
	/// <summary>
	///     Handling Interface for Cancel changes
	/// </summary>
	public interface IAcceptPendingChange
	{
		/// <summary>
		///     Can be used to cancel an value change
		/// </summary>
		event AcceptPendingChangeHandler PendingChange;
	}
}