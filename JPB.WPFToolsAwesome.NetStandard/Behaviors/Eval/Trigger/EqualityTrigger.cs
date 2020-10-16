using System.Windows;
using System.Windows.Markup;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Trigger
{
	/// <summary>
	///     Checks if both bindings are equal
	/// </summary>
	public class EqualityTrigger : TriggerStepBase
	{
		public static readonly DependencyProperty LeftProperty;

		public static readonly DependencyProperty RightProperty;

		static EqualityTrigger()
		{
			LeftProperty = Register(
				nameof(Left), typeof(object), typeof(EqualityTrigger), new PropertyMetadata(default(object)));
			RightProperty = Register(
				nameof(Right), typeof(object), typeof(EqualityTrigger), new PropertyMetadata(default(object)));
		}

		/// <summary>
		///		The object or binding to be compared with <see cref="Left"/>
		/// </summary>
		public object Right
		{
			get { return GetValue(RightProperty); }
			set { SetValue(RightProperty, value); }
		}
		
		/// <summary>
		///		The object or binding to be compared with <see cref="Right"/>
		/// </summary>
		public object Left
		{
			get { return GetValue(LeftProperty); }
			set { SetValue(LeftProperty, value); }
		}

		public override bool Evaluate(object dataContext, DependencyObject dependencyObject)
		{
			return Equals(Left, Right);
		}
	}
}