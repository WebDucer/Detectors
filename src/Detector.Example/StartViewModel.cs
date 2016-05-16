using Prism.Mvvm;
using System.Windows.Input;

namespace Detector.Example {
  public class StartViewModel : BindableBase {
    public ICommand StartCommand { get; set; }
  }
}
