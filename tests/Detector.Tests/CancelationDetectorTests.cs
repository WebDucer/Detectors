using de.webducer.net.Detector.Base;
using de.webducer.net.Detector.BaseDetectors;
using FluentAssertions;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace de.webducer.net.Detector.Tests {
  [TestFixture]
  public class CancelDetectorTests : BaseDetectorTests<CancelationDetector<bool>, bool> {
    private CancellationTokenSource _source;

    [SetUp]
    public void TestInit() {
      _source = new CancellationTokenSource();
    }

    [TearDown]
    public void TestCleanup() {
      if (_source != null) {
        _source.Dispose();
      }
      _source = null;
    }

    [Test]
    [Category("Validation")]
    public void CancelationDetector_NotFinished_ShouldHaveValidCancelationToken() {
      // Arrange
      var detector = new CancelationDetector<bool>(_source.Token);

      // Act
      var sutTask = detector.Start();
      Task.Delay(1).Wait();
      detector.Cleanup();
      sutTask.Wait(DefaultWaitTimeout).Should().BeTrue("Task should be completed (check Cleanup)");
      var sut = sutTask.Result;

      // Assert
      _source.IsCancellationRequested.Should().BeFalse();
      sut.State.Should().Be(ResultState.DetectorNotFinished);
    }

    #region override

    protected override ResultState GetDefaultPositiveState() {
      return ResultState.UserCanceled;
    }

    protected override CancelationDetector<bool> GetShortRunningDetector() {
      return new CancelationDetector<bool>(_source.Token, positiveResult: true);
    }

    protected override CancelationDetector<bool> GetLongRunningDetector() {
      return new CancelationDetector<bool>(_source.Token, positiveResult: true);
    }

    protected override void TriggerPositiveResult() {
      _source.Cancel();
    }

    protected override bool GetPositiveResult() {
      return true;
    }

    #endregion
  }

}
