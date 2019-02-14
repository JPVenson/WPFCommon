using System;

namespace JPB.ErrorValidation.ValidationTyps
{
	public enum AsyncState
	{
		/// <summary>
		/// Let the Engine decide
		/// </summary>
		NoPreference = -1,
		/// <summary>
		/// The IValidation Element will be executed in the callers context together will all other sync elements
		/// </summary>
		Sync = 0,
		/// <summary>
		/// The IValidation element will be executed in its own Task
		/// </summary>
		Async = 1,
		/// <summary>
		/// The IValidation element will be executed together will all other elements from that call.
		/// </summary>
		AsyncSharedPerCall = 2,
		/// <summary>
		/// The IValidation element will be executed later in the dispatcher
		/// </summary>
		SyncToDispatcher = 3
	}

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