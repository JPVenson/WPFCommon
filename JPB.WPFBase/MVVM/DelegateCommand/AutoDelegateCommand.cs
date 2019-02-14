using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace JPB.WPFBase.MVVM.DelegateCommand
{
	public class AutoDelegateCommand : DelegateCommandBase
	{
		public AutoDelegateCommand(Action execute, Expression<Func<bool>> canExecute) :
			base(execute, canExecute.Compile())
		{
			UsedProperties = new HashSet<string>();
			EvaluateCanExecute(canExecute);
		}

		private HashSet<string> UsedProperties { get; set; }

		private void EvaluateCanExecute(Expression<Func<bool>> canExecute)
		{
			
		}

		public override event EventHandler CanExecuteChanged;

		protected virtual void OnCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}