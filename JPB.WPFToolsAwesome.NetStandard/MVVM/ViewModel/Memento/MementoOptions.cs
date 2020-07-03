using System;

namespace JPB.WPFToolsAwesome.MVVM.ViewModel.Memento
{
	/// <summary>
	///     Contains all Memento Options for a ViewModel. Set the Default to enable all NEW created memento using ViewModels to
	///     use the given options
	/// </summary>
	public struct MementoOptions
	{
		static MementoOptions()
		{
			Default = new MementoOptions { WeakData = true };
		}

		public MementoOptions(bool tryCloneData, bool weakData, bool captureByINotifyPropertyChanged, ResolvePropertyOnMomentHost resolveProperty)
		{
			TryCloneData = tryCloneData;
			WeakData = weakData;
			CaptureByINotifyPropertyChanged = captureByINotifyPropertyChanged;
			ResolveProperty = resolveProperty;
		}

		public static MementoOptions Default { get; set; }

		public ResolvePropertyOnMomentHost ResolveProperty { get; set; }

		/// <summary>
		///     If set the <code>MementoViewModelBase</code> listens to the INotifyPropertyChanged event
		/// </summary>
		public bool CaptureByINotifyPropertyChanged { get; set; }

		/// <summary>
		///		If the Data impliments <see cref="ICloneable"/> it will try to clone the data
		/// </summary>
		public bool TryCloneData { get; set; }

		/// <summary>
		///     If set to true reference types will be stored as Weak references
		/// </summary>
		public bool WeakData { get; set; }
	}
}