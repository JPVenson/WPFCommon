using System;
using System.Windows;

namespace JPB.WPFBase.Behaviors.Eval.Evaluators
{
    /// <summary>
    ///     Compares both values and checks where the left value is less then the right value
    /// </summary>
    public class LessThenEvaluator : EvaluatorStepBase
    {
        static LessThenEvaluator()
        {
            LeftProperty = Register(
                "Left", typeof(int), typeof(LessThenEvaluator), new PropertyMetadata(default(int)));
            RightProperty = Register(
                "Right", typeof(int), typeof(LessThenEvaluator), new PropertyMetadata(default(int)));
        }

        public static readonly DependencyProperty LeftProperty;

        public static readonly DependencyProperty RightProperty;

        public IComparable Right
        {
            get { return (int)GetValue(RightProperty); }
            set { SetValue(RightProperty, value); }
        }

        public IComparable Left
        {
            get { return (int)GetValue(LeftProperty); }
            set { SetValue(LeftProperty, value); }
        }

        public override bool Evaluate(object dataContext)
        {
            return Left.CompareTo(Right) < 0;
        }
    }
}