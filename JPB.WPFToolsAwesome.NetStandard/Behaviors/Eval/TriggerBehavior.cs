using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using JPB.WPFToolsAwesome.Behaviors.Eval.Actions;
using JPB.WPFToolsAwesome.Behaviors.Eval.Trigger;
using Microsoft.Xaml.Behaviors;

namespace JPB.WPFToolsAwesome.Behaviors.Eval
{
	/// <summary>
	///		Evaluates the <see cref="TriggerStep"/> and if its true runs the <see cref="TriggerActions"/>
	/// </summary>
	[ContentProperty(nameof(TriggerStep))]
	public class TriggerBehavior : Behavior<DependencyObject>
	{
		public static readonly DependencyProperty TriggerStepProperty;
		public static readonly DependencyProperty TriggerActionsProperty;
		public static readonly DependencyProperty TriggerExecutionTypeProperty;

		static TriggerBehavior()
		{
			TriggerStepProperty = DependencyProperty.Register(
				nameof(TriggerStep), typeof(ITriggerStep), typeof(TriggerBehavior), new PropertyMetadata(default(ITriggerStep)));
			TriggerActionsProperty = DependencyProperty.Register(
				nameof(TriggerActions), typeof(TriggerActionCollection), typeof(TriggerBehavior),
				new PropertyMetadata(default(TriggerActionCollection)));
			TriggerExecutionTypeProperty = DependencyProperty.Register(
				nameof(TriggerExecutionType), typeof(TriggerExecutionType), typeof(TriggerBehavior), new PropertyMetadata(default(TriggerExecutionType)));
			IsRunningTriggersProperty = DependencyProperty.Register(
				nameof(IsRunningTriggers), typeof(bool), typeof(TriggerBehavior), new PropertyMetadata(default(bool)));
		}

		public TriggerBehavior()
		{
			TriggerActions = new TriggerActionCollection();
		}

		public static readonly DependencyProperty IsRunningTriggersProperty;

		public bool IsRunningTriggers
		{
			get { return (bool) GetValue(IsRunningTriggersProperty); }
			set { SetValue(IsRunningTriggersProperty, value); }
		}

		public TriggerExecutionType TriggerExecutionType
		{
			get { return (TriggerExecutionType) GetValue(TriggerExecutionTypeProperty); }
			set { SetValue(TriggerExecutionTypeProperty, value); }
		}

		/// <summary>
		///		Contains the list of all <see cref="EvaluateActionBase"/> that are executed when the <see cref="TriggerStep"/> s condition is met
		/// </summary>
		public TriggerActionCollection TriggerActions
		{
			get { return (TriggerActionCollection)GetValue(TriggerActionsProperty); }
			set { SetValue(TriggerActionsProperty, value); }
		}

		/// <summary>
		///		Will be executed whenever the <see cref="TriggerStep"/> triggers <see cref="INotifyPropertyChanged.PropertyChanged"/>
		/// </summary>
		public ITriggerStep TriggerStep
		{
			get { return (ITriggerStep)GetValue(TriggerStepProperty); }
			set { SetValue(TriggerStepProperty, value); }
		}
		
		/// <summary>
		///		Runs all <see cref="TriggerActions"/>
		/// </summary>
		/// <param name="value"></param>
		protected virtual void SetValue(bool value)
		{
			foreach (var action in TriggerActions)
			{
				action.SetValue(AssociatedObject, GetDataContext(), value);
			}
		}

		protected virtual object GetDataContext()
		{
			if (AssociatedObject is FrameworkElement frameworkElement)
			{
				return frameworkElement.DataContext;
			}
			if (AssociatedObject is FrameworkContentElement frameworkContentElement)
			{
				return frameworkContentElement.DataContext;
			}

			return null;
		}

		protected virtual void AttachDataContextChangeHandler()
		{
			if (AssociatedObject is FrameworkElement frameworkElement)
			{
				frameworkElement.DataContextChanged += FrameworkElement_DataContextChanged;
			}
			if (AssociatedObject is FrameworkContentElement frameworkContentElement)
			{
				frameworkContentElement.DataContextChanged += FrameworkElement_DataContextChanged;
			}
		}

		protected virtual void FrameworkElement_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null)
			{
				TriggerStep.SetDataContext(e.NewValue);
			}
		}

		protected override void OnAttached()
		{
			TriggerStep.PropertyChanged += TriggerStep_PropertyChanged;
			var dataContext = GetDataContext();
			if (dataContext != null)
			{
				TriggerStep.SetDataContext(dataContext);
			}
			AttachDataContextChangeHandler();
			SetValue(TriggerStep.Evaluate(dataContext, AssociatedObject));
		}

		protected override void OnDetaching()
		{
			TriggerStep.PropertyChanged -= TriggerStep_PropertyChanged;
		}

		private void TriggerStep_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			CheckAccess();
			PresentationTraceSources.DataBindingSource.TraceInformation($"Trigger execution requested by changing '{e.PropertyName}'");
			if (TriggerExecutionType == TriggerExecutionType.Immediate)
			{
				SetValue(TriggerStep.Evaluate(GetDataContext(), AssociatedObject));
			}
			else
			{
				if (IsRunningTriggers)
				{
					PresentationTraceSources.DataBindingSource.TraceInformation("Stopped double trigger scheduling as an execution is already scheduled");
					return;
				}

				IsRunningTriggers = true;
				Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(() =>
				{
					IsRunningTriggers = false;
					SetValue(TriggerStep.Evaluate(GetDataContext(), AssociatedObject));
				}));
			}
		}
	}
}