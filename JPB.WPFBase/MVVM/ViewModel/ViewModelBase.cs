﻿#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
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

		internal NotificationCollector DeferredNotification { get; set; }

		/// <summary>
		///		Can be used to Defer all calls of INotifyProperty changed until the returned IDisposable is Disposed.
		///		If there is already an notification gathering in process, the same handle will be returned
		/// </summary>
		/// <returns></returns>
		[NotNull]
		[MustUseReturnValue]
		[PublicAPI]
		public virtual IDisposable DeferNotification()
		{
			if (DeferredNotification != null)
			{
				return DeferredNotification;
			}
			return DeferredNotification = new NotificationCollector(this);
		}

		/// <summary>
		///		Resumes the Notification push
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public virtual void ResumeNotification()
		{
			DeferredNotification?.Dispose();
		}

		/// <summary>
		///     Raises the accept pending change.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="newValue">The new value.</param>
		/// <returns></returns>
		[PublicAPI]
		protected virtual bool RaiseAcceptPendingChange(
			[CanBeNull]string propertyName,
			[CanBeNull]object newValue)
		{
			var e = new AcceptPendingChangeEventArgs(propertyName, newValue);
			var handler = PendingChange;
			if (handler != null)
			{
				foreach (var @delegate in handler.GetInvocationList())
				{
					@delegate.DynamicInvoke(this, e);
					if (e.CancelPendingChange)
					{
						return false;
					}
				}
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
		public virtual void SetProperty<TArgument>(ref TArgument member, TArgument value,
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
		public virtual void SetProperty<TProperty>(ref TProperty member, TProperty value, Expression<Func<TProperty>> property)
		{
			SetProperty(ref member, value, GetPropertyName(property));
		}

		/// <summary>
		///     Raises this ViewModels PropertyChanged event
		/// </summary>
		/// <param name="propertyName">Name of the property that has a new value</param>
		[NotifyPropertyChangedInvocator("propertyName")]
		[PublicAPI]
		public virtual void SendPropertyChanged([CallerMemberName] string propertyName = null)
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
			if (DeferredNotification != null)
			{
				DeferredNotification.SendNotifications.Add(e.PropertyName);
				return;
			}

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
		public virtual void SendPropertyChanged<TProperty>(Expression<Func<TProperty>> property)
		{
			SendPropertyChanged(GetPropertyName(property));
		}

		/// <summary>
		///     Raises this ViewModels PropertyChanged event
		/// </summary>
		/// <param name="propertyName">Name of the property that has a new value</param>
		[PublicAPI]
		public virtual void SendPropertyChanging([CallerMemberName] string propertyName = null)
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
		public virtual void SendPropertyChanging<TProperty>(Expression<Func<TProperty>> property)
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
			var lambda = (LambdaExpression)property;

			MemberExpression memberExpression;
			var body = lambda.Body as UnaryExpression;

			if (body != null)
			{
				var unaryExpression = body;
				memberExpression = (MemberExpression)unaryExpression.Operand;
			}
			else
			{
				memberExpression = (MemberExpression)lambda.Body;
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
			var lambda = (LambdaExpression)property;

			MemberExpression memberExpression;
			var body = lambda.Body as UnaryExpression;

			if (body != null)
			{
				var unaryExpression = body;
				memberExpression = (MemberExpression)unaryExpression.Operand;
			}
			else
			{
				memberExpression = (MemberExpression)lambda.Body;
			}

			return memberExpression.Member as PropertyInfo;
		}

		#region INotifyPropertyChanged Members

		/// <inheritdoc />
		public virtual event PropertyChangedEventHandler PropertyChanged;

		/// <inheritdoc />
		public virtual event PropertyChangingEventHandler PropertyChanging;

		/// <inheritdoc />
		public virtual event AcceptPendingChangeHandler PendingChange;

		#endregion
	}
}