using de.webducer.net.Detector.Base;
using System;

namespace de.webducer.net.Detector.Interfaces {
  public interface ITaskResult<T> {
    /// <summary>
    /// State of the Result
    /// </summary>
    ResultState State { get; }

    /// <summary>
    /// Extra result informations
    /// </summary>
    T Result { get; }

    /// <summary>
    /// Exception is set, if State is Exception
    /// </summary>
    Exception Exception { get; }
  }
}
