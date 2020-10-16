using System.Windows;
using System.Windows.Media.Animation;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Actions
{
	/// <summary>
	///		Starts a storyboard
	/// </summary>
	public class BeginStoryboardAction : TriggerActionBase
	{
		static BeginStoryboardAction()
		{
			StoryboardProperty = DependencyProperty.Register(
				nameof(Storyboard), typeof(Storyboard), typeof(BeginStoryboardAction), new PropertyMetadata(default(Storyboard)));
		}

		public static readonly DependencyProperty StoryboardProperty;

		public Storyboard Storyboard
		{
			get { return (Storyboard) GetValue(StoryboardProperty); }
			set { SetValue(StoryboardProperty, value); }
		}

		public override void SetValue(DependencyObject control, object dataContext, bool state)
		{
			Storyboard.Begin();
		}
	}
}