using Detector.Example.Events;

namespace Detector.Example.Tests.TestEvents {
  public class VoltageTestEvent : VoltageEvent, ISubscriberCounter {
    public int SubscriberCount {
      get {
        return Subscriptions.Count;
      }
    }
  }
}
