using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JPB.WPFBase.Behaviors.Eval.Actions
{
	/// <summary>
	///		base class for all actions that will be executed if all <see cref="EvaluateActionBase"/> in a <see cref="EvaluatePropertyBehavior"/> are met
	/// </summary>
	public abstract class EvaluateActionBase : DependencyObject
	{
		public abstract void SetValue(DependencyObject control, object dataContext, bool state);
	}
}
