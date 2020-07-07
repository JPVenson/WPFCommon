using System.ComponentModel;
using System.Windows;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Trigger
{
	public interface ITriggerStep : INotifyPropertyChanged
	{
		bool Evaluate(object dataContext, DependencyObject dependencyObject);
		void SetDataContext(object dataContext);
	}
}