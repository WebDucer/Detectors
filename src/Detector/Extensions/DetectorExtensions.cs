using de.webducer.net.Detector.Base;
using de.webducer.net.Detector.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace de.webducer.net.Detector.Extensions {
  public static class DetectorExtensions {
    /// <summary>
    /// Get result of the first successfull finished detector from list and cleanup all detectors
    /// </summary>
    /// <typeparam name="T">Result type</typeparam>
    /// <param name="detectors">List of detectors</param>
    /// <param name="afterStart">Action to be executed after detectors started (DEFAULT: null)</param>
    /// <param name="errorLog">Action to log errors (DEFAULT: null)</param>
    /// <returns>Result of the first finished detector</returns>
    public static async Task<ITaskResult<T>> GetResult<T>(this IEnumerable<IDetector<T>> detectors,
      Action afterStart = null, Action<string> errorLog = null) {
      // Transform to array, if not array
      var detectorArray = detectors as IDetector<T>[] ?? detectors.ToArray();

      // Start all detectors
      var startedTasks = detectors.Select(s => s.Start()).ToArray();

      // Execute after start action, if any
      afterStart?.Invoke();

      // Default result is exception
      ITaskResult<T> result = new TaskResult<T>(ResultState.Exception);

      try {
        var firstFinishedTask = await Task.WhenAny(startedTasks);
        result = firstFinishedTask.Result;
      }
      catch (Exception ex) {
        // log error
        errorLog?.Invoke(ex.Message);

        // Set exception
        result = new TaskResult<T>(ResultState.Exception, ex: ex);
      }

      // Cleanup all detectors
      try {
        Parallel.ForEach(detectorArray, detector => detector.Cleanup());
        await Task.WhenAll(startedTasks);
      }
      catch (Exception ex) {
        // Do nothing, we have the result and want to cleanup all tasks. Only logging
        errorLog?.Invoke(ex.Message);
      }

      return result;
    }
  }
}
