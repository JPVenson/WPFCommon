using System;
using System.Diagnostics;
using System.Windows.Input;

namespace JPB.WPFBase.MVVM.DelegateCommand
{
    public class DelegateCommand : ICommand
    {
        protected DelegateCommand()
        {

        }

        /// <summary>
        ///     Predicate to determine if the command is valid for execution
        /// </summary>
        protected internal Func<object, bool> CanExecutePredicate;

        /// <summary>
        ///     Action to be performed when this command is executed
        /// </summary>
        protected internal Action<object> ExecutionAction;

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
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            ExecutionAction = execute;
            CanExecutePredicate = canExecute;
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

        #region ICommand Members

        /// <summary>
        ///     Raised when CanExecute is changed
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        ///     Executes the delegate backing this DelegateCommand
        /// </summary>
        /// <param name="parameter">parameter to pass to predicate</param>
        /// <returns>True if command is valid for execution</returns>
        public bool CanExecute(object parameter)
        {
            return CanExecutePredicate == null || CanExecutePredicate(parameter);
        }

        /// <summary>
        ///     Executes the delegate backing this DelegateCommand
        /// </summary>
        /// <param name="parameter">parameter to pass to delegate</param>
        /// <exception cref="InvalidOperationException">Thrown if CanExecute returns false</exception>
        public void Execute(object parameter)
        {
            if (!CanExecute(parameter))
            {
                throw new InvalidOperationException(
                    "The command is not valid for execution, check the CanExecute method before attempting to execute.");
            }

            ExecutionAction(parameter);
        }

        #endregion
    }

    public class DelegateCommand<TParameter> : DelegateCommand
    {
	    public static readonly Func<TParameter, bool> True = (obj) => true;

        /// <inheritdoc />
        public DelegateCommand(Action execute) : this((obj) => execute())
        {
        }

        /// <inheritdoc />
        public DelegateCommand(Action<TParameter> execute) : this(execute, True)
        {
        }

        /// <inheritdoc />
        public DelegateCommand(Action<TParameter> execute, Func<TParameter, bool> canExecute)
        {
            base.ExecutionAction = (obj) =>
            {
                if (obj is TParameter)
                {
                    execute((TParameter)obj);
                }
                else
                {
                    throw new InvalidOperationException(
                        "The Execute method detected an invalid obj type in the Arguments");
                }
            };

            base.CanExecutePredicate = (obj) =>
            {
	            if (canExecute == True)
		            return true;

                if (obj is TParameter)
                {
                    return canExecute((TParameter)obj);
                }
	            return false;
            };
        }

        /// <inheritdoc />
        public DelegateCommand(Action execute, Func<bool> canExecute) : this(obj => execute(), (obj) => canExecute())
        {
        }
    }
}