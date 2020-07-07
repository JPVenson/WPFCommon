using System;
using System.Windows;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Trigger
{
	/// <summary>
	///     Compares both values and checks where the left value is less then the right value
	/// </summary>
	public class LessThenTrigger : TriggerStepBase
	{
		public static readonly DependencyProperty LeftProperty;

		public static readonly DependencyProperty RightProperty;

		static LessThenTrigger()
		{
			LeftProperty = Register(
				"Left", typeof(int), typeof(LessThenTrigger), new PropertyMetadata(default(int)));
			RightProperty = Register(
				"Right", typeof(int), typeof(LessThenTrigger), new PropertyMetadata(default(int)));
		}

		public IComparable Right
		{
			get { return (int) GetValue(RightProperty); }
			set { SetValue(RightProperty, value); }
		}

		public IComparable Left
		{
			get { return (int) GetValue(LeftProperty); }
			set { SetValue(LeftProperty, value); }
		}

		public override bool Evaluate(object dataContext, DependencyObject dependencyObject)
		{
			return Left.CompareTo(Right) < 0;
		}
	}
}