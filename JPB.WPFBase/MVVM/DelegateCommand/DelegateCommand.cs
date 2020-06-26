using System;
using System.Windows.Input;

namespace JPB.WPFBase.MVVM.DelegateCommand
{
	/// <summary>
	///     The Default implementation that uses the <seealso cref="CommandManager" /> for raising its CanExecute event
	/// </summary>
	/// <seealso cref="System.Windows.Input.ICommand" />
	public class DelegateCommand : DelegateCommandBase
	{
		/// <summary>
		///     Initializes a new instance of the DelegateCommand class.
		///     The command will always be valid for execution.
		/// </summary>
		/// <param name="execute">The delegate to call on execution</param>
		public DelegateCommand(Action execute)
			: this(execute, () => true)
		{
		}

		/// <summary>
		///     Initializes a new instance of the DelegateCommand class.
		///     The command will always be valid for execution.
		/// </summary>
		/// <param name="execute">The delegate to call on execution</param>
		public DelegateCommand(Action<object> execute)
			: this(execute, obj => true)
		{
		}

		/// <summary>
		///     Initializes a new instance of the DelegateCommand class.
		/// </summary>
		/// <param name="execute">The delegate to call on execution</param>
		/// <param name="canExecute">The predicate to determine if command is valid for execution</param>
		public DelegateCommand(Action<object> execute, Func<object, bool> canExecute)
			: base(execute, canExecute)
		{
		}

		/// <summary>
		///     Initializes a new instance of the DelegateCommand class.
		/// </summary>
		/// <param name="execute">The delegate to call on execution</param>
		/// <param name="canExecute">The predicate to determine if command is valid for execution</param>
		public DelegateCommand(Action execute, Func<bool> canExecute)
			: this(obj => execute(), obj => canExecute())
		{
		}

		/// <summary>
		///		Holds the underlying events for a manual event trigger
		/// </summary>
		protected virtual event EventHandler ManualCanExecuteChanged;

		/// <inheritdoc />
		public override event EventHandler CanExecuteChanged
		{
			add
			{
				CommandManager.RequerySuggested += value;
				ManualCanExecuteChanged += value;
			}
			remove
			{
				CommandManager.RequerySuggested -= value;
				ManualCanExecuteChanged -= value;
			}
		}

		/// <summary>
		///		Allows the raising of the CanExecuteChanged event in addition to the <see cref="CommandManager.RequerySuggested"/> event.
		///		It is not necessary to call this method in most cases as the <see cref="CommandManager.RequerySuggested"/> will raise this event
		/// </summary>
		public virtual void RaiseCanExecuteChanged()
		{
			ManualCanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	/// <summary>
	///     The Default implementation that uses the <seealso cref="CommandManager" /> for raising its CanExecute event.
	///     Strong Typed argument. If the parameter from Execute or CanExecute does not match the type the command will not be
	///     executed
	/// </summary>
	/// <typeparam name="TParameter">The type of the parameter.</typeparam>
	/// <seealso cref="System.Windows.Input.ICommand" />
	public class DelegateCommand<TParameter> : DelegateCommand
	{
		public static readonly Func<TParameter, bool> True = obj => true;

		/// <inheritdoc />
		public DelegateCommand(Action execute) : this(obj => execute())
		{
		}

		/// <inheritdoc />
		public DelegateCommand(Action<TParameter> execute) : this(execute, True)
		{
		}

		/// <inheritdoc />
		public DelegateCommand(Action<TParameter> execute, Func<TParameter, bool> canExecute)
			: base(obj =>
			{
				if (obj is TParameter)
				{
					execute((TParameter)obj);
				}
			}, obj =>
			{
				if (canExecute == True)
				{
					return true;
				}

				if (obj is TParameter)
				{
					return canExecute((TParameter)obj);
				}

				return false;
			})
		{
		}

		/// <inheritdoc />
		public DelegateCommand(Action execute, Func<bool> canExecute) : this(obj => execute(), obj => canExecute())
		{
		}
	}
}