using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using JPB.WPFBase.Behaviors.Eval.Actions;
using JPB.WPFBase.Behaviors.Eval.Evaluators;
using Microsoft.Xaml.Behaviors;

namespace JPB.WPFBase.Behaviors.Eval
{
	/// <summary>
	///		Will set the Visibility the ether <see cref="Visibility.Visible"/> or <see cref="Visibility.Collapsed"/>
	///		based on the result of the <see cref="EvaluatePropertyBehavior.EvaluateActions"/>
	/// </summary>
	public class EvaluateVisibilityPropertyBehavior : EvaluatePropertyBehavior
	{
		public EvaluateVisibilityPropertyBehavior()
		{
			EvaluateActions.Add(new SetControlPropertyFieldNameAction()
			{
				Converter = new BooleanToVisibilityConverter(),
				FieldName = nameof(ContentControl.Visibility) 
			});
		}
	}

	/// <summary>
	///		Evaluates the <see cref="EvaluatorStep"/> and if its true runs the <see cref="EvaluateActions"/>
	/// </summary>
	[ContentProperty("EvaluatorStep")]
	public class EvaluatePropertyBehavior : Behavior<DependencyObject>
	{
		public static readonly DependencyProperty EvaluatorStepProperty;

		static EvaluatePropertyBehavior()
		{
			EvaluatorStepProperty = DependencyProperty.Register(
				"EvaluatorStep", typeof(IEvaluatorStep), typeof(EvaluatePropertyBehavior), new PropertyMetadata(default(IEvaluatorStep)));
			EvaluateActionsProperty = DependencyProperty.Register(
			"EvaluateActions", typeof(EvaluateActionCollection), typeof(EvaluatePropertyBehavior), new PropertyMetadata(default(EvaluateActionCollection)));
		}

		public EvaluatePropertyBehavior()
		{
			EvaluateActions = new EvaluateActionCollection();
		}

		public static readonly DependencyProperty EvaluateActionsProperty;
		/// <summary>
		///		Contains the list of all <see cref="EvaluateActionBase"/> that are executed when the <see cref="EvaluatorStep"/> s condition is met
		/// </summary>
		public EvaluateActionCollection EvaluateActions
		{
			get { return (EvaluateActionCollection)GetValue(EvaluateActionsProperty); }
			set { SetValue(EvaluateActionsProperty, value); }
		}

		/// <summary>
		///		Will be executed whenever the <see cref="EvaluatorStep"/> triggers <see cref="INotifyPropertyChanged.PropertyChanged"/>
		/// </summary>
		public IEvaluatorStep EvaluatorStep
		{
			get { return (IEvaluatorStep)GetValue(EvaluatorStepProperty); }
			set { SetValue(EvaluatorStepProperty, value); }
		}

		/// <summary>
		///		Runs all <see cref="EvaluateActions"/>
		/// </summary>
		/// <param name="value"></param>
		public void SetValue(bool value)
		{
			foreach (var action in EvaluateActions)
			{
				action.SetValue(AssociatedObject, GetDataContext(), value);
			}
		}

		private object GetDataContext()
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

		private void AttachDataContextChangeHandler()
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

		private void FrameworkElement_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null)
			{
				EvaluatorStep.SetDataContext(e.NewValue);
			}
		}

		protected override void OnAttached()
		{
			EvaluatorStep.PropertyChanged += EvaluatorStep_PropertyChanged;
			var dataContext = GetDataContext();
			if (dataContext != null)
			{
				EvaluatorStep.SetDataContext(dataContext);
			}
			AttachDataContextChangeHandler();
			SetValue(EvaluatorStep.Evaluate(dataContext));
		}

		protected override void OnDetaching()
		{
			EvaluatorStep.PropertyChanged -= EvaluatorStep_PropertyChanged;
		}

		private void EvaluatorStep_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			SetValue(EvaluatorStep.Evaluate(GetDataContext()));
		}
	}

	public class EvaluateActionCollection : List<EvaluateActionBase>
	{
	}
}
