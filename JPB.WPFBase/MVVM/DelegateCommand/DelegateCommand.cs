using System;
using System.Collections.Generic;
using System.Windows.Input;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.WPFBase.MVVM.DelegateCommand
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

    public class AsyncDelegateCommand : AsyncViewModelBase, ICommand
    {
        /// <summary>
        /// Gets the list this Command depends on
        /// </summary>
        public List<AsyncDelegateCommand> DependsOn { get; private set; }

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
        public AsyncDelegateCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the DelegateCommand class.
        /// </summary>
        /// <param name="execute">The delegate to call on execution</param>
        /// <param name="canExecute">The predicate to determine if command is valid for execution</param>
        public AsyncDelegateCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _executionAction = execute;
            _canExecutePredicate = canExecute;

            DependsOn = new List<AsyncDelegateCommand>();
        }

        private bool _asyncCanExecute;

        public bool AsyncCanExecute
        {
            get { return _asyncCanExecute; }
            set
            {
                _asyncCanExecute = value;
                SendPropertyChanged(() => AsyncCanExecute);
            }
        }

        public override bool OnTaskException(Exception exception)
        {
            asyncCanExResult = false;
            return base.OnTaskException(exception);
        }

        #region ICommand Members

        /// <summary>
        /// Raised when CanExecute is changed
        /// </summary>
        event EventHandler ICommand.CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        private bool? asyncCanExResult = false;

        /// <summary>
        ///     Executes the delegate backing this DelegateCommand
        /// </summary>
        /// <param name="parameter">parameter to pass to predicate</param>
        /// <returns>True if command is valid for execution</returns>
        public bool CanExecute(object parameter)
        {
            if (IsWorking)
                return false;
            
            if (AsyncCanExecute && asyncCanExResult.HasValue)
            {
                var val = asyncCanExResult.Value;
                asyncCanExResult = null;
                return val;
            }

            if (AsyncCanExecute && _canExecutePredicate != null)
            {
                base.SimpleWork(() =>
                {
                    return _canExecutePredicate(parameter);
                }, s =>
                {
                    asyncCanExResult = s;
                    CommandManager.InvalidateRequerySuggested();
                });

                return false;
            }
            else if (_canExecutePredicate != null)
            {
                return _canExecutePredicate(parameter);
            }

            foreach (var asyncDelegateCommand in DependsOn)
            {
                var canExecute = asyncDelegateCommand.CanExecute(parameter);
                if (!canExecute)
                    return false;
            }

            return true;
        }

        /// <summary>
        ///     Executes the delegate backing this DelegateCommand
        /// </summary>
        /// <param name="parameter">parameter to pass to delegate</param>
        /// <exception cref="InvalidOperationException">Thrown if CanExecute returns false</exception>
        public void Execute(object parameter)
        {
            if (IsWorking)
                return;

            if (!CanExecute(parameter))
            {
                throw new InvalidOperationException(
                    "The command is not valid for execution, check the CanExecute method before attempting to execute.");
            }
            base.SimpleWork(() =>
            {
                _executionAction(parameter);
            });
        }

        #endregion
    }
}