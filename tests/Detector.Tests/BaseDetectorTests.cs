using NUnit.Framework;
using System;
using System.Threading.Tasks;
using FluentAssertions;
using de.webducer.net.Detector.Interfaces;
using de.webducer.net.Detector.Base;

namespace de.webducer.net.Detector.Tests {
  [TestFixture]
  public abstract class BaseDetectorTests<TDetector, TResult> where TDetector : IDetector<TResult> {
    protected readonly TimeSpan DefaultWaitTimeout = TimeSpan.FromSeconds(2);

    [Test]
    [Category("Validation")]
    public void NotFinishedDetected_AfterCleanup_ShouldHave_State_NotFinisched() {
      // Arrange
      var detector = GetLongRunningDetector();

      // Act
      var sut = detector.Start();
      Task.Delay(1).Wait();
      detector.Cleanup();

      // Assert
      sut.Wait(DefaultWaitTimeout).Should().BeTrue("Task should be completed (check Cleanup)");
      var result = sut.Result;
      result.State.Should().Be(ResultState.DetectorNotFinished);
    }

    [Test]
    [Category("LongRunning")]
    public void Detector_ShouldHave_DefaultPositiveState_AfterFinished() {
      // Arrange
      var detector = GetShortRunningDetector();

      // Act
      var sutTask = detector.Start();
      Task.Delay(1).Wait();
      TriggerPositiveResult();

      sutTask.Wait(DefaultWaitTimeout).Should().BeTrue("Task should be completed (check correctness of positive state and handling)");

      var sut = sutTask.Result;

      detector.Cleanup();

      // Assert
      sut.State.Should().Be(GetDefaultPositiveState());
    }

    [Test]
    [Category("Validation")]
    public void Detector_WithResult_ShouldHave_Result_OnSuccess() {
      // Arrange
      var expected = GetPositiveResult();
      var detector = GetShortRunningDetector();

      // Act
      var sutTask = detector.Start();
      Task.Delay(1).Wait();
      TriggerPositiveResult();
      sutTask.Wait(DefaultWaitTimeout).Should().BeTrue("Task should be completed (check positive state handling)");
      var sut = sutTask.Result;

      // Assert
      sut.Result.Should().Be(expected)
        .And.NotBe(default(TResult));
    }

    [Test]
    [Category("Validation")]
    public void Detector_WithResult_ShouldHave_NoResult_OnCleanup() {
      // Arrange
      var detector = GetLongRunningDetector();

      // Act
      var sut = detector.Start();
      Task.Delay(1).Wait();
      detector.Cleanup();

      // Assert
      sut.Wait(DefaultWaitTimeout).Should().BeTrue("Task should be completed (check Cleanup)");
      var result = sut.Result;
      result.Result.Should().Be(default(TResult))
        .And.NotBe(GetPositiveResult());
    }

    protected abstract TDetector GetLongRunningDetector();

    protected abstract TDetector GetShortRunningDetector();

    protected abstract ResultState GetDefaultPositiveState();

    protected abstract TResult GetPositiveResult();

    protected abstract void TriggerPositiveResult();
  }
}
