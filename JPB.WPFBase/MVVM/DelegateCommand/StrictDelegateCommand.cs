using System;
using System.Windows.Input;

namespace JPB.WPFBase.MVVM.DelegateCommand
{
	/// <summary>
	///     The Default implementation that uses the <seealso cref="CommandManager" /> for raising its CanExecute event.
	///     Strong Typed argument. If the parameter from Execute or CanExecute does not match the type the command will not be
	///     executed and an exception will be raised.
	/// </summary>
	/// <typeparam name="TParameter">The type of the parameter.</typeparam>
	/// <seealso cref="System.Windows.Input.ICommand" />
	public class StrictDelegateCommand<TParameter> : DelegateCommand
	{
		public static readonly Func<TParameter, bool> True = obj => true;

		/// <inheritdoc />
		public StrictDelegateCommand(Action execute) : this(obj => execute())
		{
		}

		/// <inheritdoc />
		public StrictDelegateCommand(Action<TParameter> execute) : this(execute, True)
		{
		}

		/// <inheritdoc />
		public StrictDelegateCommand(Action<TParameter> execute, Func<TParameter, bool> canExecute)
			: base(obj =>
			{
				if (obj is TParameter)
				{
					execute((TParameter) obj);
				}
				else
				{
					throw new InvalidOperationException(
						"The Execute method detected an invalid obj type in the Arguments");
				}
			}, obj =>
			{
				if (canExecute == True)
				{
					return true;
				}

				if (obj is TParameter)
				{
					return canExecute((TParameter) obj);
				}

				return false;
			})
		{
		}

		/// <inheritdoc />
		public StrictDelegateCommand(Action execute, Func<bool> canExecute) : this(obj => execute(), obj => canExecute())
		{
		}
	}
	
	
}