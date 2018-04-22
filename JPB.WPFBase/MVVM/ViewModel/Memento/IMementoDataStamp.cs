namespace JPB.WPFBase.MVVM.ViewModel.Memento
{
	/// <summary>
	///		Strategy for Storeing a single Memento moment.
	/// </summary>
	public interface IMementoDataStamp
	{
		/// <summary>
		///		If returns true the current IMementoDataStamp implimentation will be preserved when a Snapshot is created
		/// </summary>
		bool PreserveTypeInSnapshot { get; }

		/// <summary>
		///		Should return the unmodified moment of the data.
		/// </summary>
		/// <returns></returns>
		object GetData();
		/// <summary>
		///		Should set the internal data
		/// </summary>
		/// <param name="data"></param>
		void CaptureData(object data);
		/// <summary>
		///		If returns true the data is still the same as when captured
		/// </summary>
		/// <returns></returns>
		bool CanGetData();
		/// <summary>
		///		If returns true the given object can be stored
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		bool CanSetData(object data);

		/// <summary>
		///		The DataStamp should forget the Data
		/// </summary>
		void Forget();
	}
}