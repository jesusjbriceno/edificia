using Edificia.Shared.Result;
using FluentAssertions;

namespace Edificia.Domain.Tests.Shared;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        var error = Error.Failure("Test.Error", "Something failed");
        var result = Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Success_Generic_ShouldReturnValue()
    {
        var result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Failure_Generic_ShouldThrowWhenAccessingValue()
    {
        var error = Error.NotFound("Item", "Not found");
        var result = Result.Failure<int>(error);

        result.IsFailure.Should().BeTrue();
        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversion_ShouldCreateSuccessResult()
    {
        Result<string> result = "hello";

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void ValidationFailure_ShouldContainMultipleErrors()
    {
        var errors = new[]
        {
            Error.Validation("Title", "Title is required"),
            Error.Validation("Email", "Email is invalid")
        };

        var result = Result.ValidationFailure(errors);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors[0].Code.Should().Contain("Title");
    }
}
