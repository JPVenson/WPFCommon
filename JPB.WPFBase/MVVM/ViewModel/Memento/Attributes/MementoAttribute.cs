using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.WPFBase.MVVM.ViewModel.Memento.Attributes
{
	public abstract class MementoAttribute : Attribute
	{
	}

	/// <summary>
	///		If decorates a Property, no moments will be collected for it.
	///		If decorates a Class, no moments for propertys on this class will be collected (all propertys from base or higher objects will still be collected)
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
	public sealed class IgnoreMementoAttribute : MementoAttribute
	{

	}
}
