using System.ComponentModel;
using System.Windows;
using JPB.WPFToolsAwesome.Error.ViewModelProvider.Base;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Evaluators
{
	/// <summary>
	///		Evaluates against the <see cref="AsyncErrorProviderBase"/> and checks if the view model has any errors
	/// </summary>
	public class HasErrorEvaluator : EvaluatorStepBase
	{
		public override void SetDataContext(object dataContext)
		{
			if (dataContext is AsyncErrorProviderBase errorProvider)
			{
				WeakEventManager<INotifyDataErrorInfo, DataErrorsChangedEventArgs>
					.RemoveHandler(errorProvider, nameof(INotifyDataErrorInfo.ErrorsChanged), ErrorProvider_ErrorsChanged);
				WeakEventManager<INotifyDataErrorInfo, DataErrorsChangedEventArgs>
					.AddHandler(errorProvider, nameof(INotifyDataErrorInfo.ErrorsChanged), ErrorProvider_ErrorsChanged);
			}
			base.SetDataContext(dataContext);
		}

		public override bool Evaluate(object dataContext)
		{
			if (dataContext is AsyncErrorProviderBase errorProvider)
			{
				return errorProvider.HasErrors;
			}

			return false;
		}

		private void ErrorProvider_ErrorsChanged(object sender, System.ComponentModel.DataErrorsChangedEventArgs e)
		{
			OnPropertyChanged();
		}
	}
}