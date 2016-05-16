using Detector.Example.Detectors;
using Detector.Example.Tests.TestEvents;
using System;
using de.webducer.net.Detector.Base;
using NUnit.Framework;
using FluentAssertions;
using Detector.Example.Events;

namespace Detector.Example.Tests {
  [TestFixture]
  public class RangeDetectorTests : EventBasedDetectortests<VoltageTestEvent, double, RangeDetector<VoltageTestEvent, bool>, bool> {
    [Test]
    [Category("Validation")]
    public void RangeDetector_WithoutEvent_ThrowsException() {
      // Arrange
      VoltageEvent connectEvent = null;
      var action = new Action(() => new RangeDetector<VoltageEvent, bool>(connectEvent, null, TimeSpan.FromMilliseconds(2)));

      // Assert
      action.ShouldThrowExactly<ArgumentNullException>()
         .Which.ParamName.Should().Be("valueChangedEvent");
    }

    [Test]
    [Category("Validation")]
    public void RangeDetector_WithoutRange_ThrowsException() {
      // Arrange
      var action = new Action(() => new RangeDetector<VoltageEvent, bool>(Event, null, TimeSpan.FromMilliseconds(2)));

      // Assert
      action.ShouldThrowExactly<ArgumentNullException>()
         .Which.ParamName.Should().Be("range");
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(-100)]
    [TestCase(-1000)]
    [Category("Validation")]
    public void RangeDetector_WithNotPositiveHoldTime_ThrowsException(int milliseconds) {
      // Arrange
      var range = new Range(0, 500);
      var holdTime = TimeSpan.FromMilliseconds(milliseconds);
      var action = new Action(() => new RangeDetector<VoltageEvent, bool>(Event, range, holdTime));

      // Assert
      action.ShouldThrowExactly<ArgumentException>()
         .Which.ParamName.Should().Be("timeToHold");
    }

    protected override ResultState GetDefaultPositiveState() {
      return ResultState.Success;
    }

    protected override RangeDetector<VoltageTestEvent, bool> GetLongRunningDetector() {
      return new RangeDetector<VoltageTestEvent, bool>(Event, new Range(100, 1000), TimeSpan.FromMilliseconds(100), positiveResult: true);
    }

    protected override bool GetPositiveResult() {
      return true;
    }

    protected override RangeDetector<VoltageTestEvent, bool> GetShortRunningDetector() {
      return new RangeDetector<VoltageTestEvent, bool>(Event, new Range(100, 1000), TimeSpan.FromMilliseconds(1), positiveResult: true);
    }

    protected override void TriggerPositiveResult() {
      Event.Publish(500);
    }
  }
}
