namespace JPB.WPFBase.MVVM.ViewModel.Progress
{
	/// <summary>
	///		A simple wrapper for the ComplexWork progress that can be used to indicate a percentage process 
	/// </summary>
	public class PercentProgressInfo
	{
		public PercentProgressInfo(int progress, int maxProgress, string progressText)
		{
			Progress = progress;
			MaxProgress = maxProgress;
			ProgressText = progressText;
		}

		/// <summary>
		///		The percentage of the current work done
		/// </summary>
		public int Progress { get; }

		/// <summary>
		///		The maximum value for progress
		/// </summary>
		public int MaxProgress { get; }

		/// <summary>
		///		An optional Text for the current operation
		/// </summary>
		public string ProgressText { get; }
	}
}