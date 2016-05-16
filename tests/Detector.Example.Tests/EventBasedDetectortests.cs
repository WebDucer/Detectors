using System;
using de.webducer.net.Detector.Tests;
using Detector.Example.Tests.TestEvents;
using NUnit.Framework;
using Prism.Events;
using System.Threading.Tasks;
using FluentAssertions;
using de.webducer.net.Detector.Interfaces;

namespace Detector.Example.Tests {
  [TestFixture]
  public abstract class EventBasedDetectortests<TEvent, TPayload, TDetector, TResult> : BaseDetectorTests<TDetector, TResult>
    where TEvent : PubSubEvent<TPayload>, ISubscriberCounter, new()
    where TDetector : IDetector<TResult> {
    protected readonly static TimeSpan _MAX_WAIT_TIME = TimeSpan.FromSeconds(1);
    private IEventAggregator _eventAggregator;
    protected TEvent Event;

    [SetUp]
    public void TestInit() {
      _eventAggregator = new EventAggregator();
      Event = _eventAggregator.GetEvent<TEvent>();
    }

    [TearDown]
    public void TestCleanup() {
      _eventAggregator = null;
      Event = null;
    }

    [Test]
    [Category("Validation")]
    public void NoSubscription_BeforeDetectorStart() {
      // Arrange
      var sut = GetShortRunningDetector();
      const int expectedSubscriptions = 0;

      // Act
      Task.Delay(1).Wait();

      // Assert
      Event.SubscriberCount.Should().Be(expectedSubscriptions);
    }

    [Test]
    [Category("Validation")]
    public void OnlyOneSubscription_AfterStart() {
      // Arrange
      var sut = GetShortRunningDetector();
      const int expectedSubscriptions = 1;

      // Act
      sut.Start();
      Task.Delay(1).Wait();

      // Assertion
      Event.SubscriberCount.Should().Be(expectedSubscriptions);

      sut.Cleanup();
    }

    [Test]
    [Category("Validation")]
    public void NoSubscriptions_AfterCleanup() {
      // Arrange
      var sut = GetShortRunningDetector();
      const int expectedSubscriptions = 0;

      // Act
      var sutTask = sut.Start();
      Task.Delay(1).Wait();
      sut.Cleanup();
      sutTask.Wait(_MAX_WAIT_TIME).Should().BeTrue("Detector should be finished in very short time (possible deadlock)");

      // Assert
      Event.SubscriberCount.Should().Be(expectedSubscriptions);
    }

    [Test]
    [Category("Validation")]
    public void NoSubscriptions_OnSuccess() {
      // Arrange
      var sut = GetShortRunningDetector();
      const int expectedSubscriptions = 0;

      // Act
      var sutTask = sut.Start();
      Task.Delay(1).Wait();
      TriggerPositiveResult();
      Task.Delay(1).Wait();
      sutTask.Wait(_MAX_WAIT_TIME).Should().BeTrue("Detector should be finished in very short time (possible deadlock)");

      // Assert
      Event.SubscriberCount.Should().Be(expectedSubscriptions);
    }
  }
}
