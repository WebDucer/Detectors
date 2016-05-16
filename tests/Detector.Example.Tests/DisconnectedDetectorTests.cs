using Detector.Example.Tests.TestEvents;
using System;
using de.webducer.net.Detector.Base;
using NUnit.Framework;
using FluentAssertions;
using System.Threading.Tasks;
using Detector.Example.Events;
using Detector.Example.Detectors;

namespace Detector.Example.Tests {
  [TestFixture]
  public class DisconnectedDetectorTests : EventBasedDetectortests<ConnectStateTestEvent, ConnectState, DisconnectedDetector<bool>, bool> {
    [Test]
    [Category("Validation")]
    public void DisconnectDetector_WithoutEvent_ThrowsException() {
      // Arrange
      ConnectStateEvent connectEvent = null;
      var action = new Action(() => new DisconnectedDetector<bool>(connectEvent));

      // Assert
      action.ShouldThrowExactly<ArgumentNullException>()
         .Which.ParamName.Should().Be("connectEvent");
    }

    [Test]
    [Category("Validation")]
    public void PositiveResult_AfterOneSecond_WithoutEvents() {
      // Arrange
      var sut = GetShortRunningDetector();

      // Act
      var sutTask = sut.Start();
      Task.Delay(1).Wait();

      // Assert
      sutTask.Wait(TimeSpan.FromSeconds(1.5)).Should().BeTrue("Timer not triggered positive result");
    }

    protected override ResultState GetDefaultPositiveState() {
      return ResultState.Success;
    }

    protected override DisconnectedDetector<bool> GetLongRunningDetector() {
      return GetShortRunningDetector();
    }

    protected override bool GetPositiveResult() {
      return true;
    }

    protected override DisconnectedDetector<bool> GetShortRunningDetector() {
      return new DisconnectedDetector<bool>(Event, positiveResult: true);
    }

    protected override void TriggerPositiveResult() {
      Event.Publish(ConnectState.Disconnected);
    }
  }
}
