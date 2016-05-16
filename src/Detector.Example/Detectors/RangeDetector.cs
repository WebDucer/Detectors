using de.webducer.net.Detector.Base;
using Prism.Events;
using System;
using System.Timers;

namespace Detector.Example.Detectors {
  public class RangeDetector<TEvent, T> : DetectorBase<T>
    where TEvent : PubSubEvent<double> {
    private readonly TEvent _valueChangedEvent;
    private readonly Timer _holdTimer;
    private readonly Range _range;

    public RangeDetector(TEvent valueChangedEvent, Range range, TimeSpan timeToHold, ResultState positiveState = ResultState.Success, T positiveResult = default(T)) : base(positiveState, positiveResult) {
      if (valueChangedEvent == null) {
        throw new ArgumentNullException(nameof(valueChangedEvent));
      }

      if (range == null) {
        throw new ArgumentNullException(nameof(range));
      }

      if (timeToHold.TotalMilliseconds <= 0) {
        throw new ArgumentException("Hold time should be positive", nameof(timeToHold));
      }

      _valueChangedEvent = valueChangedEvent;
      _range = range;
      _holdTimer = new Timer(timeToHold.TotalMilliseconds) {
        AutoReset = false
      };
    }

    protected override void OnStart() {
      _holdTimer.Elapsed += OnTimerElapsed;
      _valueChangedEvent.Subscribe(OnValueChanged, ThreadOption.BackgroundThread);
    }

    protected override void OnCleanup() {
      if (TcsResult.IsResultSet) {
        return;
      }

      // Deregister event
      _valueChangedEvent.Unsubscribe(OnValueChanged);

      // Release timer
      _holdTimer.Elapsed -= OnTimerElapsed;
      _holdTimer.Stop();
      _holdTimer.Close();
    }

    private void OnValueChanged(double value) {
      // Start timer, if value in range
      if (_range.IsDetected(value)) {
        // Start only, if timer not already started
        if (!_holdTimer.Enabled) {
          _holdTimer.Start();
        }
      }
      else {
        // Stop timer (value is not in range)
        _holdTimer.Stop();
      }
    }

    private void OnTimerElapsed(object sender, ElapsedEventArgs e) {
      // unsubscribe events
      _holdTimer.Elapsed -= OnTimerElapsed;
      _valueChangedEvent.Unsubscribe(OnValueChanged);

      // Set positive result
      var result = new TaskResult<T>(state: PositiveState, result: PositiveResult);

      // Set result
      TcsResult.CheckAndSetResult(result);
    }
  }
}
