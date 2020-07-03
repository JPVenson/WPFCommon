using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JPB.WPFToolsAwesome.MVVM.ViewModel.Memento.Snapshots
{
	[Serializable]
	public class MementoObjectSnapshot : ISerializable
	{
		private const string SerCountMementoData = "DataLength";
		private const string SerMementoData = "Data_{0}";
		//public const string XmlSerNamespace = "https://jean-pierre-bachmann.de/WpfTools/Memento/Xml";

		public MementoObjectSnapshot()
		{
			MementoSnapshots = new List<MementoPropertySnaptshot>();
		}

		public MementoObjectSnapshot(SerializationInfo info, StreamingContext context) : this()
		{
			using (new MementoSerialisationContext())
			{
				var countMementoData = info.GetInt32(SerCountMementoData);
				for (int i = 0; i < countMementoData; i++)
				{
					var moment = info.GetValue(string.Format(SerMementoData, i), typeof(MementoPropertySnaptshot)) as MementoPropertySnaptshot;
					MementoSnapshots.Add(moment);
				}
			}
		}

		public ICollection<MementoPropertySnaptshot> MementoSnapshots { get; internal set; }

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			using (new MementoSerialisationContext())
			{
				info.AddValue(SerCountMementoData, MementoSnapshots.Count);
				var i = 0;
				foreach (var o in MementoSnapshots)
				{
					info.AddValue(string.Format(SerMementoData, i), o, typeof(MementoPropertySnaptshot));
					i++;
				}
			}
		}
	}
}