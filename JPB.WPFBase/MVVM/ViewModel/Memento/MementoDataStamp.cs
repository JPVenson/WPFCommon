namespace JPB.WPFBase.MVVM.ViewModel.Memento
{
	public class MementoDataStamp : IMementoDataStamp
	{
		public object Data { get; set; }
		public bool Tracked { get; set; }

		public bool PreserveTypeInSnapshot { get; } = true;

		public object GetData()
		{
			return Data;
		}

		public void CaptureData(object data)
		{
			Tracked = true;
			Data = data;
		}

		public bool CanGetData()
		{
			return Tracked;
		}

		public bool CanSetData(object data)
		{
			return !Tracked;
		}

		public void Forget()
		{
			Data = null;
			Tracked = false;
		}
	}
}