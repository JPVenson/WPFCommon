using System.Windows;
using System.Windows.Media.Animation;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Actions
{
	/// <summary>
	///		Stops a storyboard
	/// </summary>
	public class StopStoryboardAction : TriggerActionBase
	{
		static StopStoryboardAction()
		{
			StoryboardProperty = DependencyProperty.Register(
				nameof(Storyboard), typeof(Storyboard), typeof(StopStoryboardAction), new PropertyMetadata(default(Storyboard)));
		}

		public static readonly DependencyProperty StoryboardProperty;

		public Storyboard Storyboard
		{
			get { return (Storyboard) GetValue(StoryboardProperty); }
			set { SetValue(StoryboardProperty, value); }
		}

		public override void SetValue(DependencyObject control, object dataContext, bool state)
		{
			Storyboard.Stop();
		}
	}
}