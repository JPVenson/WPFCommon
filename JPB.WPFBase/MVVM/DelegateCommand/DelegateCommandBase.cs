using System;
using System.Windows.Input;

namespace JPB.WPFBase.MVVM.DelegateCommand
{
	/// <summary>
	///		Base class for delegate commands
	/// </summary>
	/// <seealso cref="System.Windows.Input.ICommand" />
	public abstract class DelegateCommandBase : ICommand
	{
		public bool UseWeakReference { get; }

		/// <summary>
		///     Predicate to determine if the command is valid for execution
		/// </summary>
		protected internal WeakReference<Func<object, bool>> CanExecutePredicateReference;

		protected internal Func<object, bool> CanExecutePredicate;

		/// <summary>
		///     Action to be performed when this command is executed
		/// </summary>
		protected internal WeakReference<Action<object>> ExecutionActionReference;

		protected internal Action<object> ExecutePredicate;

		protected Func<object, bool> ObtainCanExecute()
		{
			if (UseWeakReference)
			{
				CanExecutePredicateReference.TryGetTarget(out var @delegate);
				return @delegate;
			}

			return CanExecutePredicate;
		}
		protected Action<object> ObtainExecute()
		{
			if (UseWeakReference)
			{
				ExecutionActionReference.TryGetTarget(out var @delegate);
				return @delegate;
			}

			return ExecutePredicate;
		}

		protected DelegateCommandBase()
		{
		}

		/// <summary>
		///     Initializes a new instance of the DelegateCommand class.
		///     The command will always be valid for execution.
		/// </summary>
		/// <param name="execute">The delegate to call on execution</param>
		/// <param name="useWeakReference">If set to true the delegate will be stored as a weak reference</param>
		protected DelegateCommandBase(Action execute, bool useWeakReference = false)
			: this(execute, () => true, useWeakReference)
		{
		}

		/// <summary>
		///     Initializes a new instance of the DelegateCommand class.
		///     The command will always be valid for execution.
		/// </summary>
		/// <param name="execute">The delegate to call on execution</param>
		/// <param name="useWeakReference">If set to true the delegate will be stored as a weak reference</param>
		protected DelegateCommandBase(Action<object> execute, bool useWeakReference = false)
			: this(execute, obj => true, useWeakReference)
		{
		}

		/// <summary>
		///     Initializes a new instance of the DelegateCommand class.
		/// </summary>
		/// <param name="execute">The delegate to call on execution</param>
		/// <param name="canExecute">The predicate to determine if command is valid for execution</param>
		/// <param name="useWeakReference">If set to true the delegates will be stored as a weak reference</param>
		protected DelegateCommandBase(Action<object> execute, Func<object, bool> canExecute, bool useWeakReference = false)
		{
			UseWeakReference = useWeakReference;
			if (UseWeakReference)
			{
				ExecutionActionReference = new WeakReference<Action<object>>(execute ?? throw new ArgumentNullException(nameof(execute)));
				CanExecutePredicateReference = new WeakReference<Func<object, bool>>(canExecute);
			}
			else
			{
				ExecutePredicate = execute ?? throw new ArgumentNullException(nameof(execute));
				CanExecutePredicate = canExecute;
			}
		}

		/// <summary>
		///     Initializes a new instance of the DelegateCommand class.
		/// </summary>
		/// <param name="execute">The delegate to call on execution</param>
		/// <param name="canExecute">The predicate to determine if command is valid for execution</param>
		/// <param name="useWeakReference">If set to true the delegates will be stored as a weak reference</param>
		protected DelegateCommandBase(Action execute, Func<bool> canExecute, bool useWeakReference = false)
			: this(obj => execute(), obj => canExecute(), useWeakReference)
		{
		}

		#region ICommand Members

		/// <inheritdoc />
		public abstract event EventHandler CanExecuteChanged;
		
		/// <inheritdoc />
		public virtual bool CanExecute(object parameter)
		{
			var canExecute = ObtainCanExecute();
			return CanExecutePredicate == null 
			       || (canExecute != null && canExecute(parameter));
		}
		
		/// <inheritdoc />
		public virtual void Execute(object parameter)
		{
			var execute = ObtainExecute();
			if (!CanExecute(parameter) || execute == null)
			{
				throw new InvalidOperationException(
					"The command is not valid for execution, check the CanExecute method before attempting to execute.");
			}

			execute(parameter);
		}

		#endregion
	}
}