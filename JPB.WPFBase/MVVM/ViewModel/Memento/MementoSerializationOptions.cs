using System;
using System.Reflection;

namespace JPB.WPFBase.MVVM.ViewModel.Memento
{
	public class MementoSerializationOptions
	{
		private static MementoSerializationOptions _instance;

		private MementoSerializationOptions()
		{
		}

		public static MementoSerializationOptions Instance
		{
			get { return _instance ?? (_instance = new MementoSerializationOptions()); }
		}

		/// <summary>
		///		If set, non .NET types will be emitted to the Serialized output
		/// </summary>
		public bool SaveSerialization { get; set; }

		/// <summary>
		///		Can be used to overwrite the default .Net Resolve for Assemblys
		/// </summary>
		public Func<AssemblyName, Assembly> AssemblyResolve { get; set; }		
		
		/// <summary>
		///		Can be used to overwrite the default .Net Resolve for Types
		/// </summary>
		public Func<Assembly, string, bool, Type> TypeResolve { get; set; }
	}
}