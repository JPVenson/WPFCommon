namespace JPB.WPFToolsAwesome.Error.ValidationTypes
{
	public enum AsyncRunState
	{
		/// <summary>
		///		Let the Framework decide
		/// </summary>
		NoPreference = 0,
		/// <summary>
		///		Executes one Validation item and schedules another if there is a request for it while executing the first one but not more
		/// </summary>
		CurrentPlusOne = 1,
		/// <summary>
		///		Only schedule one Validation item and reject others
		/// </summary>
		OnlyOnePerTime = 2,
	}
}