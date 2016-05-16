using Detector.Example.Events;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace Detector.Example {
  public class FakeViewModel : BindableBase {
    #region Fields
    private readonly ConnectStateEvent _connectEvent;
    private readonly VoltageEvent _voltageEvent;
    private readonly Timer _simulationTimer;
    private readonly Random _randomizer = new Random((int)DateTime.Now.TimeOfDay.Ticks);
    #endregion

    #region Constructors
    public FakeViewModel(ConnectStateEvent connectEvent, VoltageEvent voltageEvent) {
      _connectEvent = connectEvent;
      _voltageEvent = voltageEvent;
      _simulationTimer = new Timer(50) {
        AutoReset = true
      };
      _simulationTimer.Elapsed += OnTimerTick;
    }
    #endregion

    #region Properties
    private bool _isConnected = false;
    public bool IsConnected {
      get { return _isConnected; }
      set {
        SetProperty(ref _isConnected, value);
        _connectEvent.Publish(value ? ConnectState.Connected : ConnectState.Disconnected);
        InitVoltageSimulation();
      }
    }

    private double _voltageValue = 0;
    public double VoltageValue {
      get { return _voltageValue; }
      set {
        SetProperty(ref _voltageValue, value);
        _voltageEvent.Publish(value);
        IsConnected = true;
      }
    }

    private double _voltageValueBase = 0;
    public double VoltageValueBase {
      get { return _voltageValueBase; }
      set { SetProperty(ref _voltageValueBase, value); }
    }
    #endregion

    #region Helper Methods
    private void InitVoltageSimulation() {
      Task.Run(() => {
        if (IsConnected) {
          if (!_simulationTimer.Enabled) {
            _simulationTimer.Start();
          }
        }
        else {
          _simulationTimer.Stop();
        }
      });
    }

    private void OnTimerTick(object sender, ElapsedEventArgs e) {
      var baseValue = VoltageValueBase;
      var diff = (_randomizer.NextDouble() - 0.5d) * 0.2;
      VoltageValue = baseValue + diff;
    }
    #endregion
  }
}
