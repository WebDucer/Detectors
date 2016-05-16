using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;

namespace Detector.Example {
  public class MessageViewModel : BindableBase {
    private IEnumerable<string> _messages = Enumerable.Empty<string>();
    public IEnumerable<string> Messages {
      get { return _messages; }
      set { SetProperty(ref _messages, value); }
    }
  }
}
