#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

#endregion

namespace JPB.WPFBase.MVVM.ViewModel.Memento.Snapshots
{
	public sealed class MementoSerialisationContext : IDisposable
	{
		public MementoSerialisationContext()
		{
			HandleCounter++;
			_serialisationContext = _serialisationContext ?? this;
		}

		public static int HandleCounter { get; private set; }

		internal static object If(object set, object notSet)
		{
			return SerialisationContext == null ? notSet : set;
		}

		[ThreadStatic]
		private static MementoSerialisationContext _serialisationContext;

		public static MementoSerialisationContext SerialisationContext
		{
			get { return _serialisationContext; }
		}

		public void Dispose()
		{
			HandleCounter--;
			if (HandleCounter == 0)
			{
				_serialisationContext = null;
			}
			else if (HandleCounter < 0)
			{
				throw new InvalidOperationException("You cannot dispose the MementoSerialisationContext more then you created it. You called Dispose too often!.");
			}
		}
	}

	[Serializable]
	public class MementoPropertySnaptshot : ISerializable
	{
		private const string SerCountMementoData = "DataLength";
		private const string SerMementoDataType = "DataType_{0}";
		private const string SerMementoMomentType = "PreserveMomentType_{0}";
		private const string SerMementoData = "Data_{0}";
		private const string SerMementoName = "Name";
		private const string SerMementoPropTye = "Type";

		public MementoPropertySnaptshot()
		{
		}

		public MementoPropertySnaptshot(SerializationInfo info, StreamingContext context)
		{
			using (new MementoSerialisationContext())
			{
				PropertyName = info.GetString(SerMementoName);
				var propType = info.GetString(SerMementoPropTye);
				PropertyType = Type.GetType(propType, MementoSerializationOptions.Instance.AssemblyResolve,
				MementoSerializationOptions.Instance.TypeResolve);

				if (PropertyType == null)
				{
					Trace.TraceWarning($"JPB.MementoPattern: Could not DeSerialize the Snapshot for Property '{PropertyName}' As the Type '{propType}' could not be loaded");
					return;
				}

				var countMementoData = info.GetInt32(SerCountMementoData);
				MementoData = new Stack<IMementoDataStamp>();
				for (int i = 0; i < countMementoData; i++)
				{
					var mementoTypeInfo = info.GetString(string.Format(SerMementoMomentType, i));

					if (mementoTypeInfo == null)
					{
						mementoTypeInfo = typeof(MementoDataStamp).ToString();
					}
					var mementoType = Type.GetType(mementoTypeInfo, MementoSerializationOptions.Instance.AssemblyResolve,
					MementoSerializationOptions.Instance.TypeResolve);

					var typeInfo = info.GetString(string.Format(SerMementoDataType, i));

					object moment;
					if (typeInfo == null)
					{
						moment = null;
					}
					else
					{
						var type = Type.GetType(typeInfo, MementoSerializationOptions.Instance.AssemblyResolve,
						MementoSerializationOptions.Instance.TypeResolve);
						moment = info.GetValue(string.Format(SerMementoData, i), type ?? PropertyType);
					}

					var memento = (IMementoDataStamp)Activator.CreateInstance(mementoType ?? typeof(MementoDataStamp));
					if (memento.CanSetData(moment))
					{
						memento.CaptureData(moment);
						MementoData.Push(memento);
					}
				}
			}
		}

		public string PropertyName { get; internal set; }
		public Type PropertyType { get; internal set; }
		public Stack<IMementoDataStamp> MementoData { get; internal set; }

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			using (new MementoSerialisationContext())
			{
				info.AddValue(SerMementoName, PropertyName);
				info.AddValue(SerMementoPropTye, PropertyType.ToString());
				info.AddValue(SerCountMementoData, MementoData.Count);
				var i = 0;
				foreach (var o in MementoData)
				{
					if (o.PreserveTypeInSnapshot)
					{
						info.AddValue(string.Format(SerMementoMomentType, i), o.GetType());
					}
					else
					{
						info.AddValue(string.Format(SerMementoMomentType, i), null);
					}

					var data = o.GetData();
					info.AddValue(string.Format(SerMementoDataType, i), data?.GetType());
					info.AddValue(string.Format(SerMementoData, i), data);
					i++;
				}
			}
		}
	}
}