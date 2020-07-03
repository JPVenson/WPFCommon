using System.Collections.Concurrent;
using System.Dynamic;

namespace JPB.WPFToolsAwesome.MVVM.ViewModel.Memento
{
	public class UiMementoProxy : DynamicObject
	{
		private readonly MementoController _controller;

		internal UiMementoProxy(MementoController controller)
		{
			_controller = controller;
			_uiMementoController = new ConcurrentDictionary<string, UiMementoController>();
		}

		private readonly ConcurrentDictionary<string, UiMementoController> _uiMementoController;

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = _uiMementoController.GetOrAdd(binder.Name, f => new UiMementoController(_controller, f));
			return true;
		}
	}
}