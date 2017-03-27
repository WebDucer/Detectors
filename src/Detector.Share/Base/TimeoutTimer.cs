using System;
using System.Threading;
using System.Threading.Tasks;

namespace de.webducer.net.Detector.Base
{
	public class TimeoutTimer : CancellationTokenSource, IDisposable
	{
		public TimeoutTimer(TimeSpan timeOut, Action callback)
		{
			if (timeOut.TotalMilliseconds < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(timeOut), "Timeout should be always positive");
			}

			Task.Delay(timeOut)
				.ContinueWith(t => callback(),
			                  CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously
			                  | TaskContinuationOptions.OnlyOnRanToCompletion,
			                  TaskScheduler.Default);
		}

		public new void Dispose()
		{
			base.Cancel();
		}
	}
}
