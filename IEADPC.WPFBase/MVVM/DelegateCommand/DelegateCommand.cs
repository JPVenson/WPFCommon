﻿using System;
using System.Windows.Input;

namespace IEADPC.WPFBase.MVVM.DelegateCommand
{
    public class DelegateCommand : ICommand
    {
        /// <summary>
        ///     Predicate to determine if the command is valid for execution
        /// </summary>
        private readonly Predicate<object> _canExecutePredicate;

        /// <summary>
        ///     Action to be performed when this command is executed
        /// </summary>
        private readonly Action<object> _executionAction;

        /// <summary>
        ///     Initializes a new instance of the DelegateCommand class.
        ///     The command will always be valid for execution.
        /// </summary>
        /// <param name="execute">The delegate to call on execution</param>
        public DelegateCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the DelegateCommand class.
        /// </summary>
        /// <param name="execute">The delegate to call on execution</param>
        /// <param name="canExecute">The predicate to determine if command is valid for execution</param>
        public DelegateCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _executionAction = execute;
            _canExecutePredicate = canExecute;
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
            return _canExecutePredicate == null || _canExecutePredicate(parameter);
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

            _executionAction(parameter);
        }

        #endregion
    }
}