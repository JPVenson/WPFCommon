namespace JPB.WPFBase.MVVM.ViewModel.Memento
{
	/// <summary>
	///     Defines how imported Moments should be processed
	/// </summary>
	public enum ImportFlags
	{
		///// <summary>
		/////		If you import an moments age it will overwrite the existing age
		///// </summary>
		//OverwriteExistingAges,
		///// <summary>
		/////		If you import an moments age it will be aged to fit after the existing age
		///// </summary>
		//ExistingFirst,
		///// <summary>
		/////		If you import an moments age existing ages will be aged to fit after the new age
		///// </summary>
		//ImportedFirst,

		/// <summary>
		///     All imported ages will be appended to the memento
		/// </summary>
		Append,

		/// <summary>
		///     All imported ages will be set before all existing moments
		/// </summary>
		Prefix
	}
}