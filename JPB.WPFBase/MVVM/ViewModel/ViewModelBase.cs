using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace JPB.WPFBase.MVVM.ViewModel
{
    public delegate void AcceptPendingChangeHandler(
       object sender,
       AcceptPendingChangeEventArgs e);

    public interface IAcceptPendingChange
    {
        event AcceptPendingChangeHandler PendingChange;
    }

    public class AcceptPendingChangeEventArgs : EventArgs
    {
        public AcceptPendingChangeEventArgs(string propertyName, object newValue)
        {
            PropertyName = propertyName;
            NewValue = newValue;
        }

        public string PropertyName { get; private set; }
        public object NewValue { get; private set; }
        public bool CancelPendingChange { get; set; }
        // flesh this puppy out
    }

    public class ViewModelBase : ThreadSaveViewModelActor, IAcceptPendingChange, INotifyPropertyChanged, INotifyPropertyChanging
    {
        public ViewModelBase(Dispatcher disp)
            : base(disp)
        {

        }

        public ViewModelBase()
        {

        }

        #region INotifyPropertyChanged Members

        /// <summary>
        ///     Raised when a property on this object has a new value
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;
        public event AcceptPendingChangeHandler PendingChange;

        #endregion

        protected virtual bool RaiseAcceptPendingChange(
            string propertyName,
            object newValue)
        {
            var e = new AcceptPendingChangeEventArgs(propertyName, newValue);
            var handler = this.PendingChange;
            if (handler != null)
            {
                handler(this, e);
                return !e.CancelPendingChange;
            }
            return true;
        }

        /// <summary>
        /// Allows to raise the AcceptPending Change for the Memento Pattern
        /// </summary>
        /// <param name="member"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        public void SetProperty<TArgument>(ref TArgument member, TArgument value, [CallerMemberName]string propertyName = null)
        {
            if (RaiseAcceptPendingChange(propertyName, value))
            {
                SendPropertyChanging(propertyName);
                member = value;
                SendPropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// Allows to raise the AcceptPending Change for the Memento Pattern
        /// </summary>
        /// <param name="member"></param>
        /// <param name="value"></param>
        /// <param name="property"></param>
        public void SetProperty<TProperty>(ref TProperty member, TProperty value, Expression<Func<TProperty>> property)
        {
            SetProperty(ref member, value, GetPropertyName(property));
        }

        /// <summary>
        ///     Raises this ViewModels PropertyChanged event
        /// </summary>
        /// <param name="propertyName">Name of the property that has a new value</param>
        public void SendPropertyChanged([CallerMemberName]string propertyName = null)
        {
            SendPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Raises this ViewModels PropertyChanged event
        /// </summary>
        /// <param name="e">Arguments detailing the change</param>
        protected virtual void SendPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Raises the PropertyChanged Event
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="property"></param>
        public void SendPropertyChanged<TProperty>(Expression<Func<TProperty>> property)
        {
            SendPropertyChanged(GetPropertyName(property));
        }


        /// <summary>
        ///     Raises this ViewModels PropertyChanged event
        /// </summary>
        /// <param name="propertyName">Name of the property that has a new value</param>
        public void SendPropertyChanging([CallerMemberName]string propertyName = null)
        {
            SendPropertyChanging(new PropertyChangingEventArgs(propertyName));
        }

        /// <summary>
        ///     Raises this ViewModels PropertyChanging event
        /// </summary>
        /// <param name="e">Arguments detailing the change</param>
        protected virtual void SendPropertyChanging(PropertyChangingEventArgs e)
        {
            var handler = PropertyChanging;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        ///     Raises this ViewModels PropertyChanging event
        /// </summary>
        /// <param name="property">Arguments detailing the change</param>
        public void SendPropertyChanging<TProperty>(Expression<Func<TProperty>> property)
        {
            SendPropertyChanging(GetPropertyName(property));
        }

        public static string GetPropertyName<TProperty>(Expression<Func<TProperty>> property)
        {
            var lambda = (LambdaExpression)property;

            MemberExpression memberExpression;
            var body = lambda.Body as UnaryExpression;

            if (body != null)
            {
                UnaryExpression unaryExpression = body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else
                memberExpression = (MemberExpression)lambda.Body;
            return memberExpression.Member.Name;
        }

        public static PropertyInfo GetProperty<TProperty>(Expression<Func<TProperty>> property)
        {
            var lambda = (LambdaExpression)property;

            MemberExpression memberExpression;
            var body = lambda.Body as UnaryExpression;

            if (body != null)
            {
                UnaryExpression unaryExpression = body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else
                memberExpression = (MemberExpression)lambda.Body;
            return memberExpression.Member as PropertyInfo;
        }
    }
}