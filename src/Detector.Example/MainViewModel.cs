using Prism.Commands;
using Prism.Mvvm;

namespace Detector.Example {
  public class MainViewModel : BindableBase {
    public MainViewModel() {
      CurrentViewModel = new StartViewModel {
        StartCommand = new DelegateCommand(StartDiagnosis)
      };
    }

    private BindableBase _currentViewModel = null;
    public BindableBase CurrentViewModel {
      get { return _currentViewModel; }
      set { SetProperty(ref _currentViewModel, value); }
    }

    private void StartDiagnosis() {
      CurrentViewModel = new DiagnosisViewModel();
    }
  }
}
