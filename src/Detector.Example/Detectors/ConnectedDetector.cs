using de.webducer.net.Detector.Base;
using Detector.Example.Events;
using Prism.Events;
using System;

namespace Detector.Example.Detectors {
  public class ConnectedDetector<T> : DetectorBase<T> {
    private readonly ConnectStateEvent _connectEvent;

    public ConnectedDetector(ConnectStateEvent connectEvent, ResultState positiveState = ResultState.Success, T positiveResult = default(T))
      : base(positiveState, positiveResult) {
      if (connectEvent == null) {
        throw new ArgumentNullException(nameof(connectEvent));
      }

      _connectEvent = connectEvent;
    }

    protected override void OnStart() {
      // Register event processing on start
      _connectEvent.Subscribe(OnConnectionStateFired, ThreadOption.BackgroundThread);
    }

    protected override void OnCleanup() {
      // Unregistre event procession on cleanup
      _connectEvent.Unsubscribe(OnConnectionStateFired);
    }

    private void OnConnectionStateFired(ConnectState state) {
      // Check if result already set
      if (TcsResult.IsResultSet) {
        // Unregister event processing
        _connectEvent.Unsubscribe(OnConnectionStateFired);
        return;
      }

      // Check for correct state
      if (state != ConnectState.Connected) {
        return;
      }

      // Process correct state
      // Unsubscribe event processing
      _connectEvent.Unsubscribe(OnConnectionStateFired);

      // Create result
      var result = new TaskResult<T>(state: PositiveState, result: PositiveResult);

      // Set result
      TcsResult.CheckAndSetResult(result);
    }
  }
}
