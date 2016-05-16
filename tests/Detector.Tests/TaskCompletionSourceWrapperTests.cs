using de.webducer.net.Detector.Base;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace de.webducer.net.Detector.Tests {
  [TestFixture]
  public class TaskCompletionSourceWrapperTests {
    [Test]
    public void NullParameter_InConstructor_ShouldRaise_NullArgumentException() {
      // Arrange
      TaskCompletionSource<bool> parameter = null;

      // Act
      var sut = new Action(() => new TaskCompletionSourceWrapper<bool>(parameter));

      // Assert
      sut.ShouldThrowExactly<ArgumentNullException>().
        Which.ParamName.Should().Be("taskCompletionSource");
    }

    [Test]
    public void Initialized_TaskCompletionSourceWrapper_ShouldHave_ValidTask() {
      // Arrange
      var parameter = new TaskCompletionSource<bool>();

      // Act
      var sut = new TaskCompletionSourceWrapper<bool>(parameter);

      // Assert
      sut.Task.Should().NotBeNull()
        .And.Be(parameter.Task);
    }

    [Test]
    public void Initialized_TaskCompletionSourceWrapper_ShouldHave_NotSetResult() {
      // Arrange
      var parameter = new TaskCompletionSource<bool>();

      // Act
      var sut = new TaskCompletionSourceWrapper<bool>(parameter);

      // Assert
      sut.IsResultSet.Should().BeFalse();
    }
  }
}
