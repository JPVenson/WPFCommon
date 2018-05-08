using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using JPB.WPFBase.MVVM.ViewModel.Memento.Snapshots;

namespace JPB.WPFBase.MVVM.ViewModel.Memento
{
	/// <summary>
	///		Captures the given data and appends additonal data to it.
	/// </summary>
	[Serializable]
	public class MementoDataStampProxy : IMementoDataStamp
	{
		private Func<object, object> _gatherAdditionalData;
		private IMementoDataStamp _valueHolder;

		/// <summary>
		///		For Serialisation only
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DebuggerHidden]
		[Obsolete("For Serialisation only", true)]
		public MementoDataStampProxy()
		{

		}

		///  <summary>
		/// 		Creates a new Proxy Data Stamp. Use the gatherAddtionalData delegate to return an object that should be keeped along the orignal datastamp such as the datetime it was created.
		/// 		each time the Capture data will be called the delegate will be invoked
		///  </summary>
		///  <param name="gatherAdditionalData"></param>
		/// <param name="valueHolder"></param>
		public MementoDataStampProxy(Func<object, object> gatherAdditionalData, IMementoDataStamp valueHolder)
		{
			_gatherAdditionalData = gatherAdditionalData ?? throw new ArgumentNullException(nameof(gatherAdditionalData));
			_valueHolder = valueHolder ?? throw new ArgumentNullException(nameof(valueHolder));
		}

		[Serializable]
		public class ProxyValueHolder
		{
			public object AdditionalData { get; set; }
			public Type ValueHolder { get; set; }
			public object DataStamp { get; set; }

			public IMementoDataStamp CreateNewValueHolder()
			{
				return Activator.CreateInstance(ValueHolder) as IMementoDataStamp;
			}
		}

		public bool PreserveTypeInSnapshot { get; } = true;
		public object AdditionalData { get; set; }

		public bool Retained { get; private set; }

		public object GetData()
		{
			return MementoSerialisationContext.If(new ProxyValueHolder()
			{
				AdditionalData = AdditionalData,
				ValueHolder = _valueHolder.GetType(),
				DataStamp = _valueHolder.GetData()
			}, _valueHolder.GetData());
		}

		public void CaptureData(object data)
		{
			if (MementoSerialisationContext.SerialisationContext != null && data is ProxyValueHolder)
			{
				var proxyValueHolder = data as ProxyValueHolder;
				_valueHolder = proxyValueHolder.CreateNewValueHolder();
				_valueHolder.CaptureData(proxyValueHolder.DataStamp);
				AdditionalData = proxyValueHolder.AdditionalData;
				Retained = true;
			}
			else
			{
				_valueHolder.CaptureData(data);
				AdditionalData = _gatherAdditionalData(data);
			}
		}

		public bool CanGetData()
		{
			return _valueHolder.CanGetData();
		}

		public bool CanSetData(object data)
		{
			return _valueHolder == null || (_valueHolder.CanSetData(data) && !Retained);
		}

		public void Forget()
		{
			_valueHolder.Forget();
			AdditionalData = null;
		}
	}

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