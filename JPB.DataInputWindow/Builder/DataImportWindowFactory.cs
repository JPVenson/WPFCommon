using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JPB.DataInputWindow.ViewModel;

namespace JPB.DataInputWindow
{
	public class DataImportWindowFactory
	{
		static DataImportWindowFactory()
		{
			DisplayTypeLookup = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(f => f.GetTypes())
				.SelectMany(e =>
					e.GetCustomAttributes<DataInputTypeAttribute>()
						.Select(g => new KeyValuePair<DataInputTypeAttribute, Type>(g, e)))
				.ToDictionary(e => e.Key.DisplayTypes, e => e.Value);

		}

		internal static IDictionary<DisplayTypes, Type> DisplayTypeLookup;

		public DataInputBuilderFassade ConstructInputWindow()
		{
			return new DataInputBuilderFassade(new DataImportViewModel());
		}
	}
}
