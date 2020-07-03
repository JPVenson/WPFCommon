using SingleDelegateCommand = JPB.WPFToolsAwesome.MVVM.DelegateCommand.DelegateCommand;

namespace JPB.WPFToolsAwesome.MVVM.ViewModel.Memento
{
	/// <summary>
	///		Defines ICommands for controlling the Moment of a single Property from the UI
	/// </summary>
	public class UiMementoController : ViewModelBase
	{
		private readonly MementoController _controller;
		private readonly string _property;

		internal UiMementoController(MementoController controller, string property)
		{
			_controller = controller;
			_property = property;

			ForgetCommand = new SingleDelegateCommand(ForgetExecute, CanForgetExecute);
			GoForthCommand = new SingleDelegateCommand(GoForthExecute, CanGoForthExecute);
			GoBackCommand = new SingleDelegateCommand(GoBackExecute, CanGoBackExecute);
		}

		public SingleDelegateCommand ForgetCommand { get; private set; }
		public SingleDelegateCommand GoForthCommand { get; private set; }
		public SingleDelegateCommand GoBackCommand { get; private set; }

		private void GoBackExecute(object sender)
		{
			_controller.GoInHistory(_property, -1);
		}

		private bool CanGoBackExecute(object sender)
		{
			return _controller.CanGoInHistory(_property, -1);
		}

		private void GoForthExecute(object sender)
		{
			_controller.GoInHistory(_property, 1);
		}

		private bool CanGoForthExecute(object sender)
		{
			return _controller.CanGoInHistory(_property, 1);
		}

		private void ForgetExecute(object sender)
		{
			_controller.Forget(_property);
		}

		private bool CanForgetExecute(object sender)
		{
			return true;
		}
	}
}