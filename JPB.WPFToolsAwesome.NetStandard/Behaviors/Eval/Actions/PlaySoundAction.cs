using System.IO;
using System.Media;
using System.Windows;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Actions
{
	/// <summary>
	///		Plays a sound
	/// </summary>
	public class PlaySoundAction : TriggerActionBase
	{
		static PlaySoundAction()
		{
			StreamProperty = DependencyProperty.Register(
				nameof(Stream), typeof(Stream), typeof(PlaySoundAction), new PropertyMetadata(default(Stream)));
		}

		public static readonly DependencyProperty StreamProperty;

		public Stream Stream
		{
			get { return (Stream) GetValue(StreamProperty); }
			set { SetValue(StreamProperty, value); }
		}
		
		public override void SetValue(DependencyObject control, object dataContext, bool state)
		{
			var player = new SoundPlayer(Stream);
			player.Play();
		}
	}
}