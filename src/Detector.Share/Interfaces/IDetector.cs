using System.Threading.Tasks;

namespace de.webducer.net.Detector.Interfaces {
  /// <summary>
  ///// Base interface for detectors
  /// </summary>
  /// <typeparam name="T">Type of the result</typeparam>
  public interface IDetector<T> {
    /// <summary>
    /// Start the detector. Should be executed very fast
    /// </summary>
    /// <returns></returns>
    Task<ITaskResult<T>> Start();

    /// <summary>
    /// Cleanup the detector (release resources, unregister events etc.)
    /// </summary>
    void Cleanup();
  }
}
