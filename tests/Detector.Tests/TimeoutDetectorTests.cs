using de.webducer.net.Detector.BaseDetectors;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using FluentAssertions;
using de.webducer.net.Detector.Base;
using System.Diagnostics;

namespace de.webducer.net.Detector.Tests {
  [TestFixture]
  public class TimeoutDetectorTests : BaseDetectorTests<TimeoutDetector<bool>, bool> {
    private static readonly TimeSpan _MINIMAL_TIMEOUT = TimeSpan.FromMilliseconds(1);
    private static readonly TimeSpan _LONG_TIMEOUT = TimeSpan.FromSeconds(1);

    [Test]
    [Category("Validation")]
    public void TimeoutDetector_WithNegativeTimeout_ShouldThrowException() {
      // Arrange
      const int negativeTime = -1;
      var sut = new Action(() => new TimeoutDetector<bool>(TimeSpan.FromMilliseconds(negativeTime)));

      // Act & Assert
      sut.ShouldThrowExactly<ArgumentException>()
         .Which.ParamName.Should().Be("timeOut");
    }

    [Test]
    [Category("Validation")]
    public void TimeoutDetector_WithZeroTimeout_ShouldThrowException() {
      // Arrange
      const int zeroTimeout = 0;
      var sut = new Action(() => new TimeoutDetector<bool>(TimeSpan.FromMilliseconds(zeroTimeout)));

      // Act & Assert
      sut.ShouldThrowExactly<ArgumentException>()
         .Which.ParamName.Should().Be("timeOut");
    }

    [Test]
    [Category("Validation")]
    public void TimeoutDetector_WithSettedPositiveState_ShouldReturnThemAsResultState() {
      // Arrange
      var detector = new TimeoutDetector<bool>(_MINIMAL_TIMEOUT, ResultState.Success);

      // Act
      var sutTask = detector.Start();
      sutTask.Wait(_MINIMAL_TIMEOUT + DefaultWaitTimeout).Should().BeTrue("Task should be completed (check correctness of positive state and handling)");
      var sut = sutTask.Result;

      // Assert
      sut.State.Should().Be(ResultState.Success);
    }

    [Test]
    [Category("Validation")]
    public void TimeoutDetector_WithShortestTimeout_ShouldFinishFirst() {
      // Arrange
      const int slowerTimer = 50;
      const int fasterTimer = 10;
      var detector1 = new TimeoutDetector<bool>(TimeSpan.FromMilliseconds(slowerTimer), ResultState.UserDefinied1);
      var detector2 = new TimeoutDetector<bool>(TimeSpan.FromMilliseconds(fasterTimer), ResultState.UserDefinied2);

      // Act
      var tasks = new[] { detector1.Start(), detector2.Start() };
      var readyTaskIndex = Task.WaitAny(tasks, DefaultWaitTimeout);
      readyTaskIndex.Should()
         .BeGreaterOrEqualTo(0, "at least one task should be completed");

      var sut = tasks[readyTaskIndex];

      // Assert
      sut.Result.State.Should().Be(ResultState.UserDefinied2);
    }

    [TestCase(10)]
    [TestCase(25)]
    [TestCase(50)]
    [Category("Validation")]
    public void TimeoutDetector_ShouldSuccess_InTimeoutTime(long milliseconds) {
      // Arrange
      const int minDelta = -2;
      var timer = new Stopwatch();
      var detector = new TimeoutDetector<bool>(TimeSpan.FromMilliseconds(milliseconds));

      // Act
      timer.Start();
      var sutTask = detector.Start();
      sutTask.Wait(DefaultWaitTimeout + TimeSpan.FromMilliseconds(milliseconds))
         .Should()
         .BeTrue("Task should be completed (check correctness of positive state and handling)");
      var sut = sutTask.Result;
      detector.Cleanup();
      timer.Stop();

      // Assert
      sut.State.Should().Be(ResultState.TimeOut);
      timer.ElapsedMilliseconds.Should()
         .BeGreaterOrEqualTo(milliseconds + minDelta);
    }

    protected override ResultState GetDefaultPositiveState() {
      return ResultState.TimeOut;
    }

    protected override TimeoutDetector<bool> GetShortRunningDetector() {
      return new TimeoutDetector<bool>(_MINIMAL_TIMEOUT, positiveResult: true);
    }

    protected override TimeoutDetector<bool> GetLongRunningDetector() {
      return new TimeoutDetector<bool>(_LONG_TIMEOUT, positiveResult: true);
    }

    protected override void TriggerPositiveResult() {
      // nothing to do
    }

    protected override bool GetPositiveResult() {
      return true;
    }
  }
}