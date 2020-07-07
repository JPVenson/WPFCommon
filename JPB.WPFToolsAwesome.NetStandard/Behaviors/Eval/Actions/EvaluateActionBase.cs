using System.Windows;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Actions
{
	/// <summary>
	///		base class for all actions that will be executed if all <see cref="EvaluateActionBase"/> in a <see cref="TriggerBehavior"/> are met
	/// </summary>
	public abstract class EvaluateActionBase : DependencyObject
	{
		public abstract void SetValue(DependencyObject control, object dataContext, bool state);
	}
}
