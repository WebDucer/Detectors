using de.webducer.net.Detector.Base;
using System;

namespace de.webducer.net.Detector.BaseDetectors {
  /// <summary>
  /// Detector to detect timeouts
  /// </summary>
  /// <typeparam name="T">Result type</typeparam>
  public class TimeoutDetector<T> : DetectorBase<T> {
    #region Fields
    private TimeoutTimer _timer;
		private readonly TimeSpan _timeOut;
    #endregion

    #region Constructors
    public TimeoutDetector(TimeSpan timeOut, ResultState positiveState = ResultState.TimeOut,
      T positiveResult = default(T)) : base(positiveState, positiveResult) {
      if (timeOut.TotalMilliseconds <= 0) {
        throw new ArgumentException("Timeout should be alsways positive", nameof(timeOut));
      }

			_timeOut = timeOut;
    }
    #endregion

    #region DetectorBase<T> Implementation
    protected override void OnCleanup() {
      // unregister event
			_timer?.Dispose();
			_timer = null;
    }

    protected override void OnStart() {
			// register timeout event
			_timer = new TimeoutTimer(_timeOut, OnTimeOut);
    }
    #endregion

    #region Helper
    private void OnTimeOut() {
      // Stop timer, if result already set
      if (TcsResult.IsResultSet) {
				_timer?.Dispose();
				_timer = null;
        return;
      }

      // Generate positive result
      var result = new TaskResult<T>(state: PositiveState, result: PositiveResult);

      // Set result in tasc completion source
      TcsResult.CheckAndSetResult(result);
    }
    #endregion
  }
}
