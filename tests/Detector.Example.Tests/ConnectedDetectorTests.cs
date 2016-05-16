using System;
using de.webducer.net.Detector.Base;
using Detector.Example.Tests.TestEvents;
using NUnit.Framework;
using FluentAssertions;
using Detector.Example.Events;
using Detector.Example.Detectors;

namespace Detector.Example.Tests {
  [TestFixture]
  public class ConnectedDetectorTests : EventBasedDetectortests<ConnectStateTestEvent, ConnectState, ConnectedDetector<bool>, bool> {
    [Test]
    [Category("Validation")]
    public void ConnectDetector_WithoutEvent_ThrowsException() {
      // Arrange
      ConnectStateEvent connectEvent = null;
      var action = new Action(() => new ConnectedDetector<bool>(connectEvent));

      // Assert
      action.ShouldThrowExactly<ArgumentNullException>()
         .Which.ParamName.Should().Be("connectEvent");
    }

    protected override ResultState GetDefaultPositiveState() {
      return ResultState.Success;
    }

    protected override ConnectedDetector<bool> GetLongRunningDetector() {
      return GetShortRunningDetector();
    }

    protected override bool GetPositiveResult() {
      return true;
    }

    protected override ConnectedDetector<bool> GetShortRunningDetector() {
      return new ConnectedDetector<bool>(Event, positiveResult: true);
    }

    protected override void TriggerPositiveResult() {
      Event.Publish(ConnectState.Connected);
    }
  }
}
