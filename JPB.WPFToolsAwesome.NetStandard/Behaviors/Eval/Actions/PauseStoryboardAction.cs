using System.Windows;
using System.Windows.Media.Animation;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Actions
{
	/// <summary>
	///		Pauses a storyboard
	/// </summary>
	public class PauseStoryboardAction : TriggerActionBase
	{
		static PauseStoryboardAction()
		{
			StoryboardProperty = DependencyProperty.Register(
				nameof(Storyboard), typeof(Storyboard), typeof(PauseStoryboardAction), new PropertyMetadata(default(Storyboard)));
		}

		public static readonly DependencyProperty StoryboardProperty;

		public Storyboard Storyboard
		{
			get { return (Storyboard) GetValue(StoryboardProperty); }
			set { SetValue(StoryboardProperty, value); }
		}

		public override void SetValue(DependencyObject control, object dataContext, bool state)
		{
			Storyboard.Pause();
		}
	}
}