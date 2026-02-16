using Edificia.Application.Auth.Commands.ChangePassword;
using Edificia.Application.Auth.DTOs;
using FluentAssertions;

namespace Edificia.Application.Tests.Auth;

public class ChangePasswordMappingTests
{
    [Fact]
    public void Create_ShouldMapUserIdAndRequestFields()
    {
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest("OldPass123!", "NewPass456!");

        var command = ChangePasswordCommand.Create(userId, request);

        command.UserId.Should().Be(userId);
        command.CurrentPassword.Should().Be("OldPass123!");
        command.NewPassword.Should().Be("NewPass456!");
    }
}
