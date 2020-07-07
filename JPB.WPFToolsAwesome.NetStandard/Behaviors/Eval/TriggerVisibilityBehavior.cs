using System.Windows;
using System.Windows.Controls;
using JPB.WPFToolsAwesome.Behaviors.Eval.Actions;

namespace JPB.WPFToolsAwesome.Behaviors.Eval
{
	/// <summary>
	///		Will set the Visibility the ether <see cref="Visibility.Visible"/> or <see cref="Visibility.Collapsed"/>
	///		based on the result of the <see cref="TriggerBehavior.TriggerActions"/>
	/// </summary>
	public class TriggerVisibilityBehavior : TriggerBehavior
	{
		public TriggerVisibilityBehavior()
		{
			TriggerActions.Add(new SetControlPropertyFieldNameAction()
			{
				Converter = new BooleanToVisibilityConverter(),
				FieldName = nameof(ContentControl.Visibility) 
			});
		}
	}
}
