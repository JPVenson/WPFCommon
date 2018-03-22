using System.Threading.Tasks;

namespace JPB.WPFBase.MVVM.ViewModel
{
	public class AsyncViewModelBaseOptions
	{
		static AsyncViewModelBaseOptions()
		{
			DefaultOptions = Default();
		}

		public AsyncViewModelBaseOptions(TaskScheduler taskScheduler, TaskFactory taskFactory)
		{
			TaskScheduler = taskScheduler;
			TaskFactory = taskFactory;
		}

		public TaskScheduler TaskScheduler { get; private set; }
		public TaskFactory TaskFactory { get; }

		public static AsyncViewModelBaseOptions DefaultOptions { get; set; }

		public static AsyncViewModelBaseOptions Default()
		{
			var scheduler = TaskScheduler.Default;
			var factory = new TaskFactory(scheduler);
			return new AsyncViewModelBaseOptions(scheduler, factory);
		}
	}
}