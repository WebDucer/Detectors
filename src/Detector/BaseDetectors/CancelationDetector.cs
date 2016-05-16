using de.webducer.net.Detector.Base;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace de.webducer.net.Detector.BaseDetectors {
  /// <summary>
  /// Detect, if user has canceled the action
  /// </summary>
  /// <typeparam name="T">User defined type for extra result data</typeparam>
  [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
  public class CancelationDetector<T> : DetectorBase<T> {
    private static readonly TimeSpan _DEFAULT_WAIT_TIME = TimeSpan.FromSeconds(1);
    private readonly CancellationToken _cancelationToken;
    private readonly CancellationTokenSource _internalTokenSource;
    private Task _dispatcherTask;

    /// <summary>
    /// Caonstructor for user cancelation detector
    /// </summary>
    /// <param name="cancelationToken">The token that can be canceld by the user</param>
    /// <param name="positiveState">State, that should be set, if user Cancel was recognized (default: Canceled)</param>
    /// <param name="positiveResult">Result that have to be set, if detectro was positive</param>
    public CancelationDetector(CancellationToken cancelationToken, ResultState positiveState = ResultState.UserCanceled,
      T positiveResult = default(T))
      : base(positiveState, positiveResult) {
      _cancelationToken = cancelationToken;
      _internalTokenSource = new CancellationTokenSource();
    }

    #region Implementation of Detector<T>

    protected override void OnStart() {
      _dispatcherTask = Task.Run(async () => {
        while (!_internalTokenSource.IsCancellationRequested) {
          try {
            // Wait for cancelation
            await Task.Delay(_DEFAULT_WAIT_TIME, _cancelationToken);
          }
          catch (OperationCanceledException) {
            // Positive result, user canceled the action
            _internalTokenSource.Cancel();

            if (TcsResult.IsResultSet) {
              return;
            }

            var result = new TaskResult<T>(PositiveState, PositiveResult);

            TcsResult.CheckAndSetResult(result);
          }
        }
      }, _internalTokenSource.Token);
    }

    protected async override void OnCleanup() {
      _internalTokenSource.Cancel();

      if (_dispatcherTask != null) {
        try {
          await _dispatcherTask;
        }
        catch (Exception) {
          // Nothing to do, dispatcher task is canceled or terminated
        }
      }

      _internalTokenSource.Dispose();
    }

    #endregion Implementation of Detector<T>
  }
}
