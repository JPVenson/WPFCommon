using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq.Expressions;

namespace JPB.WPFBase.MVVM.DelegateCommand
{
	public class AutoDelegateCommand : DelegateCommandBase
	{
		public AutoDelegateCommand(INotifyPropertyChanged owner, Action execute, Expression<Func<bool>> canExecute) :
			base(execute, canExecute.Compile())
		{
			EvaluateCanExecute(owner, canExecute);
		}
		private HashSet<string> UsedProperties { get; set; }

		private void EvaluateCanExecute(INotifyPropertyChanged execute, Expression<Func<bool>> canExecute)
		{
			execute.PropertyChanged += Execute_PropertyChanged;
			var visitor = new CanExecuteVisitor();
			visitor.Visit(canExecute);
			UsedProperties = visitor.UsedProperties;
		}

		private void Execute_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (UsedProperties.Contains(e.PropertyName))
			{
				OnCanExecuteChanged();
			}
		}

		private class CanExecuteVisitor : ExpressionVisitor
		{
			public CanExecuteVisitor()
			{
				UsedProperties = new HashSet<string>();
			}

			public HashSet<string> UsedProperties { get; }

			protected override Expression VisitMember(MemberExpression node)
			{
				UsedProperties.Add(node.Member.Name);
				return base.VisitMember(node);
			}
		}

		public override event EventHandler CanExecuteChanged;

		protected virtual void OnCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}