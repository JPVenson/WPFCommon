using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.DataInputWindow
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public class DataInputTypeAttribute : Attribute
	{
		public DataInputTypeAttribute(DisplayTypes displayTypes)
		{
			DisplayTypes = displayTypes;
		}

		public DisplayTypes DisplayTypes { get; private set; }
	}
}
