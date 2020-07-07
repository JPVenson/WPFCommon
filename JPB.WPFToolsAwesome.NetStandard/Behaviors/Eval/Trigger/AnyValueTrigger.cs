using System.Collections;
using System.Collections.Specialized;
using System.Windows;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Trigger
{
	/// <summary>
	///     Evaluates all elements in the <see cref="Items" /> binding and checks all if any of the items match the nested
	///     validators
	/// </summary>
	public class AnyValueTrigger : DelegatorTriggerStepBase
	{
		public static readonly DependencyProperty ItemsProperty;

		static AnyValueTrigger()
		{
			ItemsProperty = Register(
				"Items", typeof(IEnumerable), typeof(AnyValueTrigger), new PropertyMetadata(default(IEnumerable)));
		}

		public AnyValueTrigger()
		{
			PropertyChanging += AnyValueTrigger_PropertyChanged;
		}

		public IEnumerable Items
		{
			get { return (IEnumerable) GetValue(ItemsProperty); }
			set { SetValue(ItemsProperty, value); }
		}

		private void AnyValueTrigger_PropertyChanged(DependencyProperty arg1, object arg2, object arg3)
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

		public override bool Evaluate(object dataContext, DependencyObject dependencyObject)
		{
			foreach (var item in Items ?? new object[0])
			{
				if (TriggerStep.Evaluate(item, dependencyObject))
				{
					return true;
				}
			}

			return false;
		}
	}
}