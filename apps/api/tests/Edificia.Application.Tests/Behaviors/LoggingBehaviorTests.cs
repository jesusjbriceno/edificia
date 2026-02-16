using System.Diagnostics;
using Edificia.Application.Behaviors;
using Edificia.Shared.Result;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Edificia.Application.Tests.Behaviors;

public class LoggingBehaviorTests
{
    private readonly Mock<ILogger<LoggingBehavior<TestCommand, Result>>> _loggerMock;
    private readonly LoggingBehavior<TestCommand, Result> _behavior;

    public LoggingBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<LoggingBehavior<TestCommand, Result>>>();
        _behavior = new LoggingBehavior<TestCommand, Result>(_loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldLogInformation_OnSuccess()
    {
        var command = new TestCommand("test");
        var next = new RequestHandlerDelegate<Result>(ct => Task.FromResult(Result.Success()));

        var result = await _behavior.Handle(command, next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        VerifyLogCalled(LogLevel.Information, times: 2); // Start + end
    }

    [Fact]
    public async Task Handle_ShouldLogWarning_OnFailure()
    {
        var command = new TestCommand("test");
        var failureResult = Result.Failure(Error.Failure("Test.Error", "Something went wrong"));
        var next = new RequestHandlerDelegate<Result>(ct => Task.FromResult(failureResult));

        var result = await _behavior.Handle(command, next, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        VerifyLogCalled(LogLevel.Warning, times: 1);
    }

    [Fact]
    public async Task Handle_ShouldLogError_OnException()
    {
        var command = new TestCommand("test");
        var next = new RequestHandlerDelegate<Result>(
            ct => throw new InvalidOperationException("Test exception"));

        var act = () => _behavior.Handle(command, next, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        VerifyLogCalled(LogLevel.Error, times: 1);
    }

    [Fact]
    public async Task Handle_ShouldReturnHandlerResult_Unchanged()
    {
        var command = new TestCommand("test");
        var expected = Result.Failure(Error.NotFound("Item", "Not found"));
        var next = new RequestHandlerDelegate<Result>(ct => Task.FromResult(expected));

        var result = await _behavior.Handle(command, next, CancellationToken.None);

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task Handle_ShouldLogWarning_ForLongRunningRequest()
    {
        var command = new TestCommand("slow");
        var next = new RequestHandlerDelegate<Result>(async ct =>
        {
            await Task.Delay(600, ct); // Exceeds 500ms threshold
            return Result.Success();
        });

        await _behavior.Handle(command, next, CancellationToken.None);

        // Should log: start (Info) + success (Info) + long-running warning (Warning)
        VerifyLogCalled(LogLevel.Information, times: 2);
        VerifyLogCalled(LogLevel.Warning, times: 1);
    }

    private void VerifyLogCalled(LogLevel level, int times)
    {
        _loggerMock.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(times));
    }

    // --- Test types ---
    public sealed record TestCommand(string Value) : IRequest<Result>;
}
