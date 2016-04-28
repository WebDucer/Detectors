using de.webducer.net.Detector.Interfaces;
using System;

namespace de.webducer.net.Detector.Base {
  public class TaskResult<T> : ITaskResult<T> {
    public TaskResult(ResultState state, T result = default(T), Exception ex = null) {
      State = state;
      Exception = ex;
      Result = result;
    }

    /// <summary>
    /// Exception is set, if State is Exception
    /// </summary>
    public Exception Exception { get; private set; }

    /// <summary>
    /// Extra result informations
    /// </summary>
    public T Result { get; set; }

    /// <summary>
    /// State of the Result
    /// </summary>
    public ResultState State { get; private set; }
  }
}
