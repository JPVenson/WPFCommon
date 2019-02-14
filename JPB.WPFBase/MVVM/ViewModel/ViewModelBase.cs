#region

using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using JetBrains.Annotations;

#endregion

namespace JPB.WPFBase.MVVM.ViewModel
{
	/// <summary>
	///     Base MVVM View-Model
	/// </summary>
	public class ViewModelBase : ThreadSaveViewModelActor,
		IAcceptPendingChange,
		INotifyPropertyChanged,
		INotifyPropertyChanging
	{
		/// <inheritdoc />
		public ViewModelBase(Dispatcher dispatcher)
			: base(dispatcher)
		{
		}

		/// <inheritdoc />
		public ViewModelBase()
		{
		}

		/// <summary>
		///     Raises the accept pending change.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="newValue">The new value.</param>
		/// <returns></returns>
		[PublicAPI]
		protected virtual bool RaiseAcceptPendingChange(
			string propertyName,
			object newValue)
		{
			var e = new AcceptPendingChangeEventArgs(propertyName, newValue);
			var handler = PendingChange;
			if (handler != null)
			{
				handler(this, e);
				return !e.CancelPendingChange;
			}

			return true;
		}

		/// <summary>
		///     Allows to raise the AcceptPending Change for the Memento Pattern
		/// </summary>
		/// <param name="member"></param>
		/// <param name="value"></param>
		/// <param name="propertyName"></param>
		[PublicAPI]
		public void SetProperty<TArgument>(ref TArgument member, TArgument value,
			[CallerMemberName] string propertyName = null)
		{
			if (RaiseAcceptPendingChange(propertyName, value))
			{
				SendPropertyChanging(propertyName);
				member = value;
				SendPropertyChanged(propertyName);
			}
		}

		/// <summary>
		///     Allows to raise the AcceptPending Change for the Memento Pattern
		/// </summary>
		/// <param name="member"></param>
		/// <param name="value"></param>
		/// <param name="property"></param>
		[NotifyPropertyChangedInvocator("property")]
		[PublicAPI]
		public void SetProperty<TProperty>(ref TProperty member, TProperty value, Expression<Func<TProperty>> property)
		{
			SetProperty(ref member, value, GetPropertyName(property));
		}

		/// <summary>
		///     Raises this ViewModels PropertyChanged event
		/// </summary>
		/// <param name="propertyName">Name of the property that has a new value</param>
		[NotifyPropertyChangedInvocator("propertyName")]
		[PublicAPI]
		public void SendPropertyChanged([CallerMemberName] string propertyName = null)
		{
			SendPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		///     Raises this ViewModels PropertyChanged event
		/// </summary>
		/// <param name="e">Arguments detailing the change</param>
		[PublicAPI]
		protected virtual void SendPropertyChanged(PropertyChangedEventArgs e)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				ThreadSaveAction(() => handler(this, e));
			}
		}

		/// <summary>
		///     Raises the PropertyChanged Event
		/// </summary>
		/// <typeparam name="TProperty"></typeparam>
		/// <param name="property"></param>
		[PublicAPI]
		public void SendPropertyChanged<TProperty>(Expression<Func<TProperty>> property)
		{
			SendPropertyChanged(GetPropertyName(property));
		}

		/// <summary>
		///     Raises this ViewModels PropertyChanged event
		/// </summary>
		/// <param name="propertyName">Name of the property that has a new value</param>
		[PublicAPI]
		public void SendPropertyChanging([CallerMemberName] string propertyName = null)
		{
			SendPropertyChanging(new PropertyChangingEventArgs(propertyName));
		}

		/// <summary>
		///     Raises this ViewModels PropertyChanging event
		/// </summary>
		/// <param name="e">Arguments detailing the change</param>
		[PublicAPI]
		protected virtual void SendPropertyChanging(PropertyChangingEventArgs e)
		{
			var handler = PropertyChanging;
			if (handler != null)
			{
				ThreadSaveAction(() => handler(this, e));
			}
		}

		/// <summary>
		///     Raises this ViewModels PropertyChanging event
		/// </summary>
		/// <param name="property">Arguments detailing the change</param>
		[PublicAPI]
		public void SendPropertyChanging<TProperty>(Expression<Func<TProperty>> property)
		{
			SendPropertyChanging(GetPropertyName(property));
		}

		/// <summary>
		///     Helper for getting the Lambda Property from the expression
		/// </summary>
		/// <typeparam name="TProperty">The type of the property.</typeparam>
		/// <param name="property">The property.</param>
		/// <returns></returns>
		[PublicAPI]
		public static string GetPropertyName<TProperty>(Expression<Func<TProperty>> property)
		{
			return GetProperty(property).Name;
		}

		/// <summary>
		///     Helper for getting the property info of an expression
		/// </summary>
		/// <typeparam name="TProperty">The type of the property.</typeparam>
		/// <param name="property">The property.</param>
		/// <returns></returns>
		public static PropertyInfo GetProperty<TProperty>(Expression<Func<TProperty>> property)
		{
			var lambda = (LambdaExpression) property;

			MemberExpression memberExpression;
			var body = lambda.Body as UnaryExpression;

			if (body != null)
			{
				var unaryExpression = body;
				memberExpression = (MemberExpression) unaryExpression.Operand;
			}
			else
			{
				memberExpression = (MemberExpression) lambda.Body;
			}

			return memberExpression.Member as PropertyInfo;
		}

		/// <summary>
		///     Helper for getting the property info of an expression
		/// </summary>
		/// <typeparam name="TProperty">The type of the property.</typeparam>
		/// <typeparam name="TObject"></typeparam>
		/// <param name="property">The property.</param>
		/// <returns></returns>
		public static PropertyInfo GetProperty<TProperty, TObject>(Expression<Func<TObject, TProperty>> property)
		{
			var lambda = (LambdaExpression) property;

			MemberExpression memberExpression;
			var body = lambda.Body as UnaryExpression;

			if (body != null)
			{
				var unaryExpression = body;
				memberExpression = (MemberExpression) unaryExpression.Operand;
			}
			else
			{
				memberExpression = (MemberExpression) lambda.Body;
			}

			return memberExpression.Member as PropertyInfo;
		}

		#region INotifyPropertyChanged Members

		/// <summary>
		///     Raised when a property on this object has a new value
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		///     Raised when a Property will be changed directly after event invoke
		/// </summary>
		public event PropertyChangingEventHandler PropertyChanging;

		/// <inheritdoc />
		/// <summary>
		///     Raised when using the SetProperty() method. Can be used to cancel a change
		/// </summary>
		public event AcceptPendingChangeHandler PendingChange;

		#endregion
	}
}