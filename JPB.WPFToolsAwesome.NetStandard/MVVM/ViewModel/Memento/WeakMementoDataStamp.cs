using System;

namespace JPB.WPFToolsAwesome.MVVM.ViewModel.Memento
{
	public class WeakMementoDataStamp : IMementoDataStamp
	{
		private WeakReference _reference;

		public bool PreserveTypeInSnapshot { get; } = false;

		public object GetData()
		{
			return _reference?.Target;
		}

		public void CaptureData(object data)
		{
			if (data != null)
			{
				_reference = new WeakReference(data);
			}
		}

		public bool CanGetData()
		{
			return _reference != null;
		}

		public bool CanSetData(object data)
		{
			return CanTrackObject(data) && _reference == null;
		}

		public void Forget()
		{
			_reference = null;
		}

		public static bool CanTrackObject(object data)
		{
			return data == null || data.GetType().IsClass;
		}
	}
}