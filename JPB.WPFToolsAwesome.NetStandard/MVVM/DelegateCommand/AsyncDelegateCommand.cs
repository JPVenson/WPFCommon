using System;
using System.Threading.Tasks;
using System.Windows.Input;
using JPB.WPFToolsAwesome.MVVM.ViewModel;

namespace JPB.WPFToolsAwesome.MVVM.DelegateCommand
{
	/// <summary>
	///     The Default implementation that uses the <seealso cref="CommandManager" />
	///		for raising its CanExecute event. When it is Executed the Method will be wrapped into an Simple Work
	/// </summary>
	/// <seealso cref="System.Windows.Input.ICommand" />
	public class AsyncDelegateCommand : DelegateCommand
	{
		private readonly AsyncViewModelBase _viewModelBase;

		/// <summary>
		///     Initializes a new instance of the DelegateCommand class.
		///     The command will always be valid for execution.
		/// </summary>
		/// <param name="viewModelBase">The Parent</param>
		/// <param name="execute">The delegate to call on execution</param>
		public AsyncDelegateCommand(AsyncViewModelBase viewModelBase, Action execute)
			: this(viewModelBase, execute, () => true)
		{
		}

		/// <summary>
		///     Initializes a new instance of the DelegateCommand class.
		///     The command will always be valid for execution.
		/// </summary>
		/// <param name="viewModelBase">The Parent</param>
		/// <param name="execute">The delegate to call on execution</param>
		public AsyncDelegateCommand(AsyncViewModelBase viewModelBase, Action<object> execute)
			: this(viewModelBase, execute, obj => true)
		{
		}

		/// <summary>
		///     Initializes a new instance of the DelegateCommand class.
		/// </summary>
		/// <param name="viewModelBase">The Parent</param>
		/// <param name="execute">The delegate to call on execution</param>
		/// <param name="canExecute">The predicate to determine if command is valid for execution</param>
		public AsyncDelegateCommand(AsyncViewModelBase viewModelBase, 
			Action<object> execute, 
			Func<object, bool> canExecute)
			: base(execute, canExecute)
		{
			_viewModelBase = viewModelBase ?? throw new ArgumentNullException(nameof(viewModelBase));
		}

		/// <summary>
		///     Initializes a new instance of the DelegateCommand class.
		/// </summary>
		/// <param name="viewModelBase">The Parent</param>
		/// <param name="execute">The delegate to call on execution</param>
		/// <param name="canExecute">The predicate to determine if command is valid for execution</param>
		public AsyncDelegateCommand(AsyncViewModelBase viewModelBase, Action execute, Func<bool> canExecute)
			: this(viewModelBase, obj => execute(), obj => canExecute())
		{
		}

		/// <inheritdoc />
		public override event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		/// <summary>
		///		Awaits the execution of this Event if any is currently running
		/// </summary>
		/// <returns></returns>
		public async Task Await()
		{
			if (_isWorking != null)
			{
				await _isWorking;
			}
		}

		private volatile Task _isWorking;

		/// <inheritdoc />
		public override bool CanExecute(object parameter)
		{
			return base.CanExecute(parameter) && (_isWorking == null || _isWorking.IsCompleted);
		}

		/// <inheritdoc />
		public override void Execute(object parameter)
		{
			var execute = ObtainExecute();
			if (!CanExecute(parameter) || execute == null)
			{
				throw new InvalidOperationException(
					"The command is not valid for execution, check the CanExecute method before attempting to execute.");
			}

			_isWorking = _viewModelBase.SimpleWork(() => { execute(parameter); }, base.RaiseCanExecuteChanged);
		}
	}
}