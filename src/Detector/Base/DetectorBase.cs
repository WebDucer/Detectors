using System.Threading.Tasks;
using de.webducer.net.Detector.Interfaces;

namespace de.webducer.net.Detector.Base {
  /// <summary>
  ///// Base class for detectors
  /// </summary>
  /// <typeparam name="T">Type of the result</typeparam>
  public abstract class DetectorBase<T> : IDetector<T> {
    #region Fields
    protected readonly ResultState PositiveState;
    protected readonly T PositiveResult;
    protected readonly TaskCompletionSourceWrapper<ITaskResult<T>> TcsResult;
    #endregion

    #region Constructors
    protected DetectorBase(ResultState positiveState, T positiveResult = default(T)) {
      PositiveState = positiveState;
      PositiveResult = positiveResult;
      TcsResult = new TaskCompletionSourceWrapper<ITaskResult<T>>(new TaskCompletionSource<ITaskResult<T>>());
    }
    #endregion

    #region IDetector implementation
    /// <summary>
    /// Start the detector. Should be executed very fast
    /// </summary>
    /// <returns></returns>
    public Task<ITaskResult<T>> Start() {
      OnStart();

      return TcsResult.Task;
    }

    /// <summary>
    /// Cleanup the detector (release resources, unregister events etc.)
    /// </summary>
    public void Cleanup() {
      OnCleanup();

      if (!TcsResult.IsResultSet) {
        TcsResult.CheckAndSetResult(new TaskResult<T>(state: ResultState.DetectorNotFinished));
      }
    }
    #endregion

    /// <summary>
    /// Start the detector (as fast as possible)
    /// </summary>
    protected abstract void OnStart();

    /// <summary>
    /// Cleanaup the detector
    /// </summary>
    protected abstract void OnCleanup();
  }
}
