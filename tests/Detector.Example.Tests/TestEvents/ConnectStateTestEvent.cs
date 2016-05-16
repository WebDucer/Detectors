using Detector.Example.Events;

namespace Detector.Example.Tests.TestEvents {
  public class ConnectStateTestEvent : ConnectStateEvent, ISubscriberCounter {
    public int SubscriberCount {
      get {
        return Subscriptions.Count;
      }
    }
  }
}
