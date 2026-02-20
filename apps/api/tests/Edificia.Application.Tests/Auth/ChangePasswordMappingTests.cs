using Edificia.Application.Auth.Commands.ChangePassword;
using Edificia.Application.Auth.DTOs;
using FluentAssertions;

namespace Edificia.Application.Tests.Auth;

public class ChangePasswordMappingTests
{
    [Fact]
    public void ExplicitOperator_ShouldMapDtoFields()
    {
        var request = new ChangePasswordRequest("OldPass123!", "NewPass456!");

        var command = (ChangePasswordCommand)request;

        command.UserId.Should().Be(Guid.Empty);
        command.CurrentPassword.Should().Be("OldPass123!");
        command.NewPassword.Should().Be("NewPass456!");
    }

    [Fact]
    public void ExplicitOperator_EnrichedWithJwtContext_ShouldSetUserId()
    {
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest("OldPass123!", "NewPass456!");

        var command = (ChangePasswordCommand)request with { UserId = userId };

        command.UserId.Should().Be(userId);
    }
}
