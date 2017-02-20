using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace JPB.WPFBase.MVVM.ViewModel
{
    //public class ThreadSaveViewModelBase : ViewModelBase
    //{
    //    public ThreadSaveViewModelBase(Dispatcher fromThread)
    //        : base(fromThread)
    //    {

    //    }

    //    public ThreadSaveViewModelBase()
    //    {

    //    }

    //    /// <summary>
    //    ///     Raised when a property on this object is about to get a new value
    //    /// </summary>
    //    public event PropertyChangingEventHandler PropertyChanging;
    //    /// <summary>
    //    ///     Raised when a property on this object has a new value
    //    /// </summary>
    //    public event PropertyChangedEventHandler PropertyChanged;

    //    /// <summary>
    //    ///     Raises this ViewModels PropertyChanged event
    //    /// </summary>
    //    /// <param name="propertyName">Name of the property that has a new value</param>
    //    public void SendPropertyChanged([CallerMemberName]string propertyName = null)
    //    {
    //        SendPropertyChanged(new PropertyChangedEventArgs(propertyName));
    //    }

    //    /// <summary>
    //    ///     Raises this ViewModels PropertyChanged event
    //    /// </summary>
    //    /// <param name="e">Arguments detailing the change</param>
    //    protected virtual void SendPropertyChanged(PropertyChangedEventArgs e)
    //    {
    //        PropertyChangedEventHandler handler = PropertyChanged;
    //        if (handler != null)
    //            handler(this, e);
    //    }

    //    public void SendPropertyChanged<TProperty>(Expression<Func<TProperty>> property)
    //    {
    //        var lambda = (LambdaExpression)property;

    //        MemberExpression memberExpression;
    //        var body = lambda.Body as UnaryExpression;

    //        if (body != null)
    //        {
    //            UnaryExpression unaryExpression = body;
    //            memberExpression = (MemberExpression)unaryExpression.Operand;
    //        }
    //        else
    //            memberExpression = (MemberExpression)lambda.Body;
    //        SendPropertyChanged(memberExpression.Member.Name);
    //    }

    //    /// <summary>
    //    ///     Raises this ViewModels PropertyChanged event
    //    /// </summary>
    //    /// <param name="propertyName">Name of the property that has a new value</param>
    //    public void SendPropertyChanging(string propertyName)
    //    {
    //        SendPropertyChanging(new PropertyChangingEventArgs(propertyName));
    //    }

    //    /// <summary>
    //    ///     Raises this ViewModels PropertyChanged event
    //    /// </summary>
    //    /// <param name="e">Arguments detailing the change</param>
    //    protected virtual void SendPropertyChanging(PropertyChangingEventArgs e)
    //    {
    //        var handler = PropertyChanging;
    //        if (handler != null)
    //            handler(this, e);
    //    }

    //    public void SendPropertyChanging<TProperty>(Expression<Func<TProperty>> property)
    //    {
    //        var lambda = (LambdaExpression)property;

    //        MemberExpression memberExpression;
    //        var body = lambda.Body as UnaryExpression;

    //        if (body != null)
    //        {
    //            UnaryExpression unaryExpression = body;
    //            memberExpression = (MemberExpression)unaryExpression.Operand;
    //        }
    //        else
    //            memberExpression = (MemberExpression)lambda.Body;
    //        SendPropertyChanging(memberExpression.Member.Name);
    //    }
    //}
}