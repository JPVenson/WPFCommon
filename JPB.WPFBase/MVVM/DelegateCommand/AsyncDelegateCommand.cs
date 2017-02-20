using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.WPFBase.MVVM.DelegateCommand
{
    public enum DependLevel
    {
        /// <summary>
        /// The complete CanExecute will be used
        /// </summary>
        Complete,
        /// <summary>
        /// Only the IsWorking flag will be used. Way more performance
        /// </summary>
        WorkingOnly
    }

    public enum CheckState
    {
        UIFired,
        BeforeExecution,
        BeforeExecutionInProgress,
        AfterExecutionInProgress,
        AsyncExecutionScheduled,
        AsyncExecutionDone,
        Unknown
    }

    public class AsyncDelegateCommand : AsyncViewModelBase, ICommand
    {
        /// <summary>
        /// Gets the list this Command depends on
        /// </summary>
        public List<Tuple<AsyncDelegateCommand, DependLevel>> DependsOn { get; private set; }

        public AsyncDelegateCommand AddDependency(AsyncDelegateCommand command, DependLevel level)
        {
            DependsOn.Add(new Tuple<AsyncDelegateCommand, DependLevel>(command, level));
            return this;
        }

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

            DependsOn = new List<Tuple<AsyncDelegateCommand, DependLevel>>();
        }

        private bool _asyncCanExecute;

        /// <summary>
        /// If set the Can Execute will be invoked Async and after the compltion the RequerySuggested event will be fired
        /// </summary>
        public bool AsyncCanExecute
        {
            get { return _asyncCanExecute; }
            set
            {
                lock (Lock)
                {
                    _asyncCanExecute = value;
                    SendPropertyChanged(() => AsyncCanExecute);
                }
            }
        }

        private CheckState _stateOfCanExecute;

        public CheckState StateOfCanExecute
        {
            get { return _stateOfCanExecute; }
            private set
            {
                _stateOfCanExecute = value;
                SendPropertyChanged(() => StateOfCanExecute);
            }
        }

        protected override bool OnTaskException(Exception exception)
        {
            _asyncCanExResult = false;
            return base.OnTaskException(exception);
        }

        #region ICommand Members

        /// <summary>
        /// Raised when CanExecute is changed
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        private bool? _asyncCanExResult;

        /// <summary>
        ///     Executes the delegate backing this DelegateCommand
        /// </summary>
        /// <param name="parameter">parameter to pass to predicate</param>
        /// <returns>True if command is valid for execution</returns>
        public bool CanExecute(object parameter)
        {
            if (!AsyncCanExecute)
            {
                if (_canExecutePredicate != null)
                {
                    return _canExecutePredicate(parameter);
                }
                return true;
            }
            else
            {
                lock (Lock)
                {
                    if (StateOfCanExecute == CheckState.BeforeExecutionInProgress || StateOfCanExecute == CheckState.AsyncExecutionScheduled)
                    {
                        return false;
                    }

                    if (_asyncCanExResult.HasValue && StateOfCanExecute == CheckState.AsyncExecutionDone)
                    {
                        return _asyncCanExResult.Value;
                    }

                    if (StateOfCanExecute == CheckState.BeforeExecution)
                    {
                        StateOfCanExecute = CheckState.BeforeExecutionInProgress;
                    }
                    else
                    {
                        StateOfCanExecute = new StackTrace().GetFrame(1).GetMethod().Name.Contains("CanExecuteCommandSource") ? CheckState.UIFired : CheckState.Unknown;

                        if (IsWorking)
                            return false;
                    }

                    if (_canExecutePredicate != null && StateOfCanExecute != CheckState.BeforeExecutionInProgress)
                    {
                        StateOfCanExecute = CheckState.AsyncExecutionScheduled;
                        base.SimpleWork(() => _canExecutePredicate(parameter), s =>
                        {
                            _asyncCanExResult = s;
                            StateOfCanExecute = CheckState.AsyncExecutionDone;
                            CommandManager.InvalidateRequerySuggested();
                        });

                        return false;
                    }

                    foreach (var s in DependsOn)
                    {
                        bool canExecute = false;
                        if (s.Item2 == DependLevel.WorkingOnly)
                        {
                            canExecute = s.Item1.IsWorking;
                        }

                        if (s.Item2 == DependLevel.Complete)
                        {
                            canExecute = s.Item1.CanExecute(parameter);
                        }

                        if (canExecute)
                        {
                            return false;
                        }
                    }

                    if (StateOfCanExecute == CheckState.BeforeExecutionInProgress)
                    {
                        StateOfCanExecute = CheckState.AfterExecutionInProgress;
                    }

                    return true;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="parameter">parameter to pass to delegate</param>
        /// <exception cref="InvalidOperationException">Thrown if CanExecute returns false</exception>
        public void Execute(object parameter)
        {
            if (IsWorking)
                return;

            base.SimpleWork(() =>
            {
                var hasResult = _asyncCanExResult.HasValue;
                ResetResult();
                StateOfCanExecute = CheckState.BeforeExecution;
                var canExecute = CanExecute(parameter);
                if (!hasResult && !canExecute)
                {
                    throw new InvalidOperationException("The command is not valid for execution, check the CanExecute method before attempting to execute.");
                }
                else if (canExecute)
                {
                    _executionAction(parameter);
                }
            });
        }

        #endregion

        public void ResetResult()
        {
            _asyncCanExResult = null;
        }
    }
}