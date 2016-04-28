using System;
using System.Threading.Tasks;

namespace de.webducer.net.Detector.Base {
  /// <summary>
  /// Thread save warapper for the task completion source
  /// </summary>
  /// <typeparam name="T">Type of the result</typeparam>
  public class TaskCompletionSourceWrapper<T> {
    private readonly object _lock = new object();
    private readonly TaskCompletionSource<T> _tcs;

    public TaskCompletionSourceWrapper(TaskCompletionSource<T> taskCompletionSource) {
      _tcs = taskCompletionSource;
      IsResultSet = false;
    }

    /// <summary>
    /// TRUE: if result already set and task completion source is completed, canceled or aborded
    /// </summary>
    public bool IsResultSet { get; private set; }

    /// <summary>
    /// Set result, if the result not already set
    /// </summary>
    /// <param name="result">Result tot set</param>
    public void CheckAndSetResult(T result) {
      lock (_lock) {
        if (IsResultSet) {
          return;
        }
        IsResultSet = true;
        _tcs.SetResult(result);
      }
    }

    /// <summary>
    /// Cancel tasc completion source, if no result set
    /// </summary>
    public void CheckAndCancel() {
      lock (_lock) {
        if (IsResultSet) {
          return;
        }
        IsResultSet = true;
        _tcs.SetCanceled();
      }
    }

    /// <summary>
    /// Set exception on task completion source, if no result set
    /// </summary>
    /// <param name="ex"></param>
    public void CheckAndSetException(Exception ex) {
      lock (_lock) {
        if (IsResultSet) {
          return;
        }
        IsResultSet = true;
        _tcs.SetException(ex ?? new Exception("Task aborded with exception"));
      }
    }

    /// <summary>
    /// Underlying task of the tasc completion source
    /// </summary>
    public Task<T> Task { get { return _tcs.Task; } }
  }
}
