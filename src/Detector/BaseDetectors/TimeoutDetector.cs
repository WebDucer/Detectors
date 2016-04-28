using de.webducer.net.Detector.Base;
using System;
using System.Timers;

namespace de.webducer.net.Detector.BaseDetectors {
  /// <summary>
  /// Detector to detect timeouts
  /// </summary>
  /// <typeparam name="T">Result type</typeparam>
  public class TimeoutDetector<T> : DetectorBase<T> {
    #region Fields
    private readonly Timer _timer;
    #endregion

    #region Constructors
    public TimeoutDetector(TimeSpan timeOut, ResultState positiveState = ResultState.TimeOut,
      T positiveResult = default(T)) : base(positiveState, positiveResult) {
      if (timeOut.Milliseconds <= 0) {
        throw new ArgumentException("Timeout should be alsways positive", nameof(timeOut));
      }

      _timer = new Timer(timeOut.TotalMilliseconds) { AutoReset = false };
    }
    #endregion

    #region DetectorBase<T> Implementation
    protected override void OnCleanup() {
      // unregister event
      _timer.Elapsed -= OnTimeOut;

      // release resources
      _timer.Stop();
      _timer.Close();
    }

    protected override void OnStart() {
      // register timeout event
      _timer.Elapsed += OnTimeOut;

      // start timer
      _timer.Start();
    }
    #endregion

    #region Helper
    private void OnTimeOut(object sender, ElapsedEventArgs e) {
      // Stop timer, if result already set
      if (TcsResult.IsResultSet) {
        _timer.Elapsed -= OnTimeOut;
        _timer.Stop();
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
