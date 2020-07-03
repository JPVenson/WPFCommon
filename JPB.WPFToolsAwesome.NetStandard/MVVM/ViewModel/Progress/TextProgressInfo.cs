namespace JPB.WPFToolsAwesome.MVVM.ViewModel.Progress
{
	/// <summary>
	///		A simple wrapper for the ComplexWork progress that can be used to indicate a single string progress
	/// </summary>
	public class TextProgressInfo
	{
		public TextProgressInfo(string text)
		{
			Text = text;
		}

		/// <summary>
		///		The Text to be reported to the UI
		/// </summary>
		public string Text { get; }
	}
}
