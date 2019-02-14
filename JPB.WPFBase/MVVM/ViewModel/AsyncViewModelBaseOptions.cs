using System.Threading.Tasks;

namespace JPB.WPFBase.MVVM.ViewModel
{
	public class AsyncViewModelBaseOptions
	{
		static AsyncViewModelBaseOptions()
		{
			DefaultOptions = Default();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncViewModelBaseOptions"/> class.
		/// </summary>
		/// <param name="taskScheduler">The task scheduler.</param>
		/// <param name="taskFactory">The task factory.</param>
		public AsyncViewModelBaseOptions(TaskScheduler taskScheduler, TaskFactory taskFactory)
		{
			TaskScheduler = taskScheduler;
			TaskFactory = taskFactory;
		}

		/// <summary>
		/// Gets the task scheduler.
		/// </summary>
		/// <value>
		/// The task scheduler.
		/// </value>
		public TaskScheduler TaskScheduler { get; }

		/// <summary>
		/// Gets the task factory.
		/// </summary>
		/// <value>
		/// The task factory.
		/// </value>
		public TaskFactory TaskFactory { get; }

		/// <summary>
		/// Gets or sets the default options.
		/// </summary>
		/// <value>
		/// The default options.
		/// </value>
		public static AsyncViewModelBaseOptions DefaultOptions { get; set; }

		/// <summary>
		///		Creates a new Default set of options that contains the current Default TaskScheduler and a new TaskFactory for it.
		/// </summary>
		/// <returns></returns>
		public static AsyncViewModelBaseOptions Default()
		{
			var scheduler = TaskScheduler.Default;
			var factory = new TaskFactory(scheduler);
			return new AsyncViewModelBaseOptions(scheduler, factory);
		}
	}
}