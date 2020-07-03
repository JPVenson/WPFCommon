using System.Windows;

namespace JPB.WPFBase.Behaviors.Eval.Evaluators
{
	/// <summary>
	///		Checks if both bindings are equal
	/// </summary>
	public class EqualityEvaluator : EvaluatorStepBase
	{
		static EqualityEvaluator()
		{
			LeftProperty = Register(
				"Left", typeof(object), typeof(EqualityEvaluator), new PropertyMetadata(default(object)));
			RightProperty = Register(
				"Right", typeof(object), typeof(EqualityEvaluator), new PropertyMetadata(default(object)));
		}

		public static readonly DependencyProperty LeftProperty;

		public static readonly DependencyProperty RightProperty;
		
		public object Right
		{
			get { return GetValue(RightProperty); }
			set { SetValue(RightProperty, value); }
		}

		public object Left
		{
			get { return GetValue(LeftProperty); }
			set { SetValue(LeftProperty, value); }
		}

		public override bool Evaluate(object dataContext)
		{
			return Equals(Left, Right);
		}
	}
}