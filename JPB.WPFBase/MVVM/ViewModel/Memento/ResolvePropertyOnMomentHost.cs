using System.Reflection;

namespace JPB.WPFBase.MVVM.ViewModel.Memento
{
	public delegate PropertyInfo ResolvePropertyOnMomentHost(object momentHost, string propertyName);
}