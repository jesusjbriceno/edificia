using Edificia.Application.Auth.Commands.ChangePassword;
using Edificia.Application.Auth.Commands.Login;
using Edificia.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Edificia.Application.Tests.Auth;

public class ChangePasswordHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ILogger<ChangePasswordHandler>> _loggerMock;
    private readonly ChangePasswordHandler _handler;

    public ChangePasswordHandlerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _loggerMock = new Mock<ILogger<ChangePasswordHandler>>();

        _handler = new ChangePasswordHandler(
            _userManagerMock.Object,
            _loggerMock.Object);
    }

    private static ApplicationUser CreateTestUser(bool mustChangePassword = false)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "root@edificia.dev",
            UserName = "root@edificia.dev",
            FullName = "Root User",
            MustChangePassword = mustChangePassword,
            IsActive = true
        };
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenPasswordChangedSuccessfully()
    {
        // Arrange
        var user = CreateTestUser();
        var command = new ChangePasswordCommand(user.Id, "OldPass123!", "NewPass456!");

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldClearMustChangePasswordFlag_WhenPasswordChanged()
    {
        // Arrange
        var user = CreateTestUser(mustChangePassword: true);
        var command = new ChangePasswordCommand(user.Id, "OldPass123!", "NewPass456!");

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.MustChangePassword.Should().BeFalse();
        user.UpdatedAt.Should().NotBeNull();
        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotCallUpdate_WhenMustChangePasswordWasFalse()
    {
        // Arrange
        var user = CreateTestUser(mustChangePassword: false);
        var command = new ChangePasswordCommand(user.Id, "OldPass123!", "NewPass456!");

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand(userId, "OldPass123!", "NewPass456!");

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("InvalidCredentials");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCurrentPasswordIsWrong()
    {
        // Arrange
        var user = CreateTestUser();
        var command = new ChangePasswordCommand(user.Id, "WrongPass!", "NewPass456!");

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Code = "PasswordMismatch", Description = "Incorrect password." }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("InvalidCurrentPassword");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPasswordChangeFailsForOtherReason()
    {
        // Arrange
        var user = CreateTestUser();
        var command = new ChangePasswordCommand(user.Id, "OldPass123!", "weak");

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Code = "PasswordTooShort", Description = "Password too short." }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("PasswordChangeFailed");
    }
}
