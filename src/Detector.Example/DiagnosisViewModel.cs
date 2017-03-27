using de.webducer.net.Detector.Base;
using de.webducer.net.Detector.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using System.Threading.Tasks;
using System;
using de.webducer.net.Detector.BaseDetectors;
using System.Threading;
using Prism.Events;
using de.webducer.net.Detector.Extensions;
using Detector.Example.Detectors;
using Detector.Example.Events;

namespace Detector.Example
{
    public class DiagnosisViewModel : BindableBase
    {
        #region Fileds

        private readonly VoltageEvent _voltageEvent;
        private readonly ConnectStateEvent _connectionEvent;

        #endregion

        #region Constructors

        public DiagnosisViewModel()
        {
            IEventAggregator eventAggregator = new EventAggregator();
            _voltageEvent = eventAggregator.GetEvent<VoltageEvent>();
            _connectionEvent = eventAggregator.GetEvent<ConnectStateEvent>();
            StartDiagnosisCommand = new DelegateCommand(StartDiagnosis, CanStartDiagnosis);
            CancelCommand = new DelegateCommand(CancelDiagnosis, CanCancelDiagnosis);
            DiagnosisStarted = false;
            CurrentViewModel = new MessageViewModel
            {
                Messages = new[]
                {
                    "Start the diagnosis with the START button."
                }
            };

            Fake = new FakeViewModel(_connectionEvent, _voltageEvent);
        }

        #endregion

        #region Properties

        private FakeViewModel _fake;

        public FakeViewModel Fake
        {
            get { return _fake; }
            set { SetProperty(ref _fake, value); }
        }

        private bool _diagnosisStarted;

        public bool DiagnosisStarted
        {
            get { return _diagnosisStarted; }
            private set
            {
                SetProperty(ref _diagnosisStarted, value);
                StartDiagnosisCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
            }
        }

        private double _currentValue;

        public double CurrentValue
        {
            get { return _currentValue; }
            set { SetProperty(ref _currentValue, value); }
        }

        private DelegateCommand _startDiagnosisCommand;

        public DelegateCommand StartDiagnosisCommand
        {
            get { return _startDiagnosisCommand; }
            private set { SetProperty(ref _startDiagnosisCommand, value); }
        }

        private DelegateCommand _cancelCommand;

        public DelegateCommand CancelCommand
        {
            get { return _cancelCommand; }
            private set { SetProperty(ref _cancelCommand, value); }
        }

        protected CancellationTokenSource UserCancelationSource { get; set; }

        private BindableBase _currentViewModel;

        public BindableBase CurrentViewModel
        {
            get { return _currentViewModel; }
            private set { SetProperty(ref _currentViewModel, value); }
        }

        #endregion

        #region Command Methods

        private void StartDiagnosis()
        {
            Task.Run(async () =>
            {
                DiagnosisStarted = true;

                UserCancelationSource?.Dispose();
                UserCancelationSource = new CancellationTokenSource();

                // Task 1: Device connected
                var result = await RunDeviceConnectedTest();

                if (IsNegativeResult(result))
                {
                    return;
                }

                // Task 2: 5V voltage hold for 5 Seconds
                result = await RunVoltageTest();

                if (IsNegativeResult(result))
                {
                    return;
                }

                // Task 3: Disconnect device
                result = await RunDisconnectTest();

                if (IsNegativeResult(result))
                {
                    return;
                }

                ShowTestResultScreen();

                DiagnosisStarted = false;
            });
        }

        private bool CanStartDiagnosis()
        {
            return !DiagnosisStarted;
        }

        private void CancelDiagnosis()
        {
            UserCancelationSource.Cancel();
        }

        private bool CanCancelDiagnosis()
        {
            return DiagnosisStarted;
        }

        #endregion

        #region Helper Methods

        private async Task<ITaskResult<bool>> RunDeviceConnectedTest()
        {
            // Init UI
            CurrentViewModel = new MessageViewModel
            {
                Messages = new[]
                {
                    "Connect the test USB device to the USB!"
                }
            };

            try
            {
                var detectors = GetDetectorsForConnection();
                var result = await detectors.GetResult();

                return result;
            }
            catch (Exception ex)
            {
                return new TaskResult<bool>(ResultState.Exception, ex: ex);
            }
        }

        private async Task<ITaskResult<bool>> RunVoltageTest()
        {
            // Init UI
            CurrentViewModel = new VoltageViewModel(_voltageEvent);

            try
            {
                var detectors = GetDetectorsForVoltage();
                var result = await detectors.GetResult();

                return result;
            }
            catch (Exception ex)
            {
                return new TaskResult<bool>(ResultState.Exception, ex: ex);
            }
        }

        private async Task<ITaskResult<bool>> RunDisconnectTest()
        {
            // Init UI
            CurrentViewModel = new MessageViewModel
            {
                Messages = new[]
                {
                    "Disconnect the test USB device from the USB!"
                }
            };

            try
            {
                var detectors = GetDetectorsForDisconnection();
                var result = await detectors.GetResult();

                return result;
            }
            catch (Exception ex)
            {
                return new TaskResult<bool>(ResultState.Exception, ex: ex);
            }
        }

        private IDetector<bool>[] GetDetectorsForConnection()
        {
            return new IDetector<bool>[]
            {
                new TimeoutDetector<bool>(TimeSpan.FromMinutes(1)),
                new CancelationDetector<bool>(UserCancelationSource.Token),
                new ConnectedDetector<bool>(_connectionEvent)
            };
        }

        private IDetector<bool>[] GetDetectorsForDisconnection()
        {
            return new IDetector<bool>[]
            {
                new TimeoutDetector<bool>(TimeSpan.FromMinutes(1)),
                new CancelationDetector<bool>(UserCancelationSource.Token),
                new DisconnectedDetector<bool>(_connectionEvent)
            };
        }

        private IDetector<bool>[] GetDetectorsForVoltage()
        {
            return new IDetector<bool>[]
            {
                new TimeoutDetector<bool>(TimeSpan.FromMinutes(1)),
                new CancelationDetector<bool>(UserCancelationSource.Token),
                new DisconnectedDetector<bool>(_connectionEvent, positiveState: ResultState.UserDefinied1),
                new RangeDetector<VoltageEvent, bool>(_voltageEvent, new Range(4.5d, 5.5d), TimeSpan.FromSeconds(2))
            };
        }

        private bool IsNegativeResult(ITaskResult<bool> result)
        {
            if (result.State == ResultState.UserCanceled)
            {
                ShowUserCancelationScreen();
                return true;
            }

            if (result.State == ResultState.TimeOut)
            {
                ShowTimeoutScreen();
                return true;
            }

            if (result.State == ResultState.UserDefinied1)
            {
                ShowDisconnectScreen();
                return true;
            }

            if (result.State != ResultState.Success)
            {
                ShowErrorScreen(result.Exception);
                return true;
            }

            return false;
        }

        private void ShowUserCancelationScreen()
        {
            CurrentViewModel = new MessageViewModel
            {
                Messages = new[]
                {
                    "YOU canceled the diagnosis!",
                    "To start the new diagnosis run please press START button."
                }
            };

            DiagnosisStarted = false;
        }

        private void ShowErrorScreen(Exception exception)
        {
            CurrentViewModel = new ErrorMessageViewModel
            {
                Messages = new[]
                {
                    "An error raised during the diagnosis",
                    exception?.Message ?? "<NULL>"
                }
            };

            DiagnosisStarted = false;
        }

        private void ShowDisconnectScreen()
        {
            CurrentViewModel = new ErrorMessageViewModel
            {
                Messages = new[]
                {
                    "The device lost connection",
                    "To repeat the diagnosis please press START button."
                }
            };

            DiagnosisStarted = false;
        }

        private void ShowTimeoutScreen()
        {
            CurrentViewModel = new MessageViewModel
            {
                Messages = new[]
                {
                    "The diagnosis ended with a TIMEOUT!",
                    "To start the new diagnosis run please press START button."
                }
            };

            DiagnosisStarted = false;
        }

        private void ShowTestResultScreen()
        {
            CurrentViewModel = new ResultMessageViewModel
            {
                Messages = new[]
                {
                    "!!! OK !!!",
                    "Diagnosis finished"
                }
            };
        }

        #endregion
    }
}