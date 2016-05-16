using Detector.Example.Events;
using Prism.Mvvm;

namespace Detector.Example {
  public class VoltageViewModel : BindableBase {
    private readonly VoltageEvent _voltageEvent;

    public VoltageViewModel(VoltageEvent voltageEvent) {
      _voltageEvent = voltageEvent;

      _voltageEvent.Subscribe(OnVoltageChanged);
    }

    private double _value = 0;
    public double Value {
      get { return _value; }
      private set { SetProperty(ref _value, value); }
    }

    private void OnVoltageChanged(double value) {
      Value = value;
    }
  }
}
