using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.WPFBase.MVVM.DelegateCommand
{
    public class AutoDelegateCommand : DelegateCommand
    {
        public AutoDelegateCommand() : base((d) => { })
        {
        }

        /// <summary>
        ///     Raised when CanExecute is changed
        /// </summary>
        public new event EventHandler CanExecuteChanged;

        internal List<string> Propertys = new List<string>();

        public CanExecuteSelector Link()
        {
            CanExecutePredicate = null;
            Propertys.Clear();
            return new CanExecuteSelector(this);
        }

        public virtual void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class CanExecuteSelector
    {
        private readonly AutoDelegateCommand _command;

        internal CanExecuteSelector(AutoDelegateCommand command)
        {
            _command = command;
        }

        public ToExecuteSelector To(INotifyPropertyChanged element)
        {
            element.PropertyChanged += Element_PropertyChanged;
            return new ToExecuteSelector(_command);
        }

        private void Element_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_command.Propertys.Contains(e.PropertyName))
            {
                _command.RaiseCanExecuteChanged();
            }
        }
    }

    public class ToExecuteSelector
    {
        internal ToExecuteSelector(AutoDelegateCommand command)
        {
            When = new WhenExecuteSelector(command);
        }

        public WhenExecuteSelector When { get; }
    }

    public class WhenExecuteFurfilledSelector
    {
        private readonly AutoDelegateCommand _command;

        internal WhenExecuteFurfilledSelector(AutoDelegateCommand command)
        {
            _command = command;
        }

        public AutoDelegateCommand Execute(Action<object> item)
        {
            _command.ExecutionAction = item;
            return _command;
        }
    }

    public class WhenExecuteSelector
    {
        private readonly AutoDelegateCommand _command;
        private readonly BoolOp? _mod;

        internal WhenExecuteSelector(AutoDelegateCommand command)
        {
            _command = command;
        }

        internal WhenExecuteSelector(AutoDelegateCommand command, BoolOp mod)
        {
            _command = command;
            _mod = mod;
        }

        public PropertyPredicateSelector Property(string name)
        {
            return new PropertyPredicateSelector(_command, name, _mod);
        }
        public PropertyPredicateSelector Property<TProperty>(Expression<Func<TProperty>> property)
        {
            return Property(ViewModelBase.GetPropertyName(property));
        }

        public PropertyPredicateSelector Function
        {
            get
            {
                return Property(null);
            }
        }
    }

    public class PropertyPredicateSelector
    {
        private readonly AutoDelegateCommand _command;
        private readonly string _propName;
        private readonly BoolOp? _mod;

        internal PropertyPredicateSelector(AutoDelegateCommand command, string propName, BoolOp? mod)
        {
            _command = command;
            _propName = propName;
            _mod = mod;
        }

        public AfterPropertySelector Is(Func<bool> action)
        {
            if (_propName != null)
                _command.Propertys.Add(_propName);

            if (_command.CanExecutePredicate == null)
            {
                _command.CanExecutePredicate = (obj) => action();
            }
            else
            {
                var pre = _command.CanExecutePredicate;
                _command.CanExecutePredicate = (obj) =>
                {
                    if (_mod.HasValue)
                    {
                        switch (_mod.Value)
                        {
                            case BoolOp.And:
                                return pre(obj) && action();
                            case BoolOp.Or:
                                return pre(obj) || action();
                        }
                    }
                    return action();
                };
            }

            return new AfterPropertySelector(_command);
        }
    }

    enum BoolOp
    {
        And,
        Or
    }

    public class AfterPropertySelector
    {
        private readonly AutoDelegateCommand _command;

        public AfterPropertySelector(AutoDelegateCommand command)
        {
            _command = command;
        }

        public WhenExecuteFurfilledSelector Then
        {
            get
            {
                return new WhenExecuteFurfilledSelector(_command);
            }
        }

        public WhenExecuteSelector And
        {
            get
            {
                return new WhenExecuteSelector(_command, BoolOp.And);
            }
        }

        public WhenExecuteSelector Or
        {
            get
            {
                return new WhenExecuteSelector(_command, BoolOp.Or);
            }
        }
    }
}