using System.Collections;
using System.Collections.Specialized;
using System.Windows;

namespace JPB.WPFBase.Behaviors.Eval.Evaluators
{
	/// <summary>
	///		Evaluates all elements in the <see cref="Items"/> binding and checks all if any of the items match the nested validators
	/// </summary>
	public class AnyValueEvaluator : DelegatorEvaluatorStepBase
	{
		public static readonly DependencyProperty ItemsProperty;

		public IEnumerable Items
		{
			get { return (IEnumerable) GetValue(ItemsProperty); }
			set { SetValue(ItemsProperty, value); }
		}

		static AnyValueEvaluator()
		{
			ItemsProperty = Register(
				"Items", typeof(IEnumerable), typeof(AnyValueEvaluator), new PropertyMetadata(default(IEnumerable)));
		}

		public AnyValueEvaluator()
		{
			PropertyChanging += AnyValueEvaluator_PropertyChanged;
		}

		private void AnyValueEvaluator_PropertyChanged(DependencyProperty arg1, object arg2, object arg3)
		{
			if (arg1.Name == nameof(Items))
			{
				if (arg3 is INotifyCollectionChanged ncc)
				{
					ncc.CollectionChanged += NccOnCollectionChanged;
				}
				if (arg2 is INotifyCollectionChanged nccNew)
				{
					nccNew.CollectionChanged -= NccOnCollectionChanged;
				}
			}
		}

		private void NccOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			OnPropertyChanged(nameof(Items));
		}

		public override bool Evaluate(object dataContext)
		{
			foreach (var item in Items ?? new object[0])
			{
				if (base.EvaluatorStep.Evaluate(item))
				{
					return true;
				}
			}

			return false;
		}
	}
}