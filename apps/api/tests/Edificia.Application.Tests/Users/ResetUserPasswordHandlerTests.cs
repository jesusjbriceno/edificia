using Edificia.Application.Interfaces;
using Edificia.Application.Users;
using Edificia.Application.Users.Commands.ResetUserPassword;
using Edificia.Domain.Constants;
using Edificia.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Edificia.Application.Tests.Users;

public class ResetUserPasswordHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<ResetUserPasswordHandler>> _loggerMock;
    private readonly ResetUserPasswordHandler _handler;

    public ResetUserPasswordHandlerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<ResetUserPasswordHandler>>();

        _handler = new ResetUserPasswordHandler(
            _userManagerMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object);
    }

    private static ApplicationUser CreateTestUser()
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "architect@edificia.dev",
            UserName = "architect@edificia.dev",
            FullName = "Ana Arquitecta",
            IsActive = true,
            MustChangePassword = false
        };
    }

    [Fact]
    public async Task Handle_ShouldResetPassword_WhenRootResetsArchitect()
    {
        // Arrange
        var user = CreateTestUser();
        var rootUserId = Guid.NewGuid();
        var rootUser = new ApplicationUser { Id = rootUserId };

        var command = new ResetUserPasswordCommand(user.Id, rootUserId);

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { AppRoles.Architect });

        _userManagerMock.Setup(x => x.FindByIdAsync(rootUserId.ToString()))
            .ReturnsAsync(rootUser);

        _userManagerMock.Setup(x => x.GetRolesAsync(rootUser))
            .ReturnsAsync(new List<string> { AppRoles.Root });

        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset-token-123");

        _userManagerMock.Setup(x => x.ResetPasswordAsync(user, "reset-token-123", It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.MustChangePassword.Should().BeTrue();
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldSendResetEmail()
    {
        // Arrange
        var user = CreateTestUser();
        var rootUserId = Guid.NewGuid();
        var rootUser = new ApplicationUser { Id = rootUserId };

        var command = new ResetUserPasswordCommand(user.Id, rootUserId);

        SetupSuccessfulReset(user, rootUser, rootUserId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(
            x => x.SendAsync(
                user.Email!,
                It.Is<string>(s => s.Contains("restablecida")),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var command = new ResetUserPasswordCommand(Guid.NewGuid(), Guid.NewGuid());

        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAdminTriesToResetAdminPassword()
    {
        // Arrange
        var targetUser = CreateTestUser();
        var adminUserId = Guid.NewGuid();
        var adminUser = new ApplicationUser { Id = adminUserId };

        var command = new ResetUserPasswordCommand(targetUser.Id, adminUserId);

        _userManagerMock.Setup(x => x.FindByIdAsync(targetUser.Id.ToString()))
            .ReturnsAsync(targetUser);

        _userManagerMock.Setup(x => x.GetRolesAsync(targetUser))
            .ReturnsAsync(new List<string> { AppRoles.Admin });

        _userManagerMock.Setup(x => x.FindByIdAsync(adminUserId.ToString()))
            .ReturnsAsync(adminUser);

        _userManagerMock.Setup(x => x.GetRolesAsync(adminUser))
            .ReturnsAsync(new List<string> { AppRoles.Admin });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("CannotModifyHigherRole");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenResetPasswordFails()
    {
        // Arrange
        var user = CreateTestUser();
        var rootUserId = Guid.NewGuid();
        var rootUser = new ApplicationUser { Id = rootUserId };

        var command = new ResetUserPasswordCommand(user.Id, rootUserId);

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { AppRoles.Architect });

        _userManagerMock.Setup(x => x.FindByIdAsync(rootUserId.ToString()))
            .ReturnsAsync(rootUser);

        _userManagerMock.Setup(x => x.GetRolesAsync(rootUser))
            .ReturnsAsync(new List<string> { AppRoles.Root });

        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset-token");

        _userManagerMock.Setup(x => x.ResetPasswordAsync(user, "reset-token", It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("PasswordResetFailed");
    }

    [Fact]
    public async Task Handle_ShouldSucceed_EvenWhenEmailFails()
    {
        // Arrange
        var user = CreateTestUser();
        var rootUserId = Guid.NewGuid();
        var rootUser = new ApplicationUser { Id = rootUserId };

        var command = new ResetUserPasswordCommand(user.Id, rootUserId);

        SetupSuccessfulReset(user, rootUser, rootUserId);

        _emailServiceMock.Setup(x => x.SendAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SMTP failure"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    private void SetupSuccessfulReset(ApplicationUser user, ApplicationUser rootUser, Guid rootUserId)
    {
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { AppRoles.Architect });

        _userManagerMock.Setup(x => x.FindByIdAsync(rootUserId.ToString()))
            .ReturnsAsync(rootUser);

        _userManagerMock.Setup(x => x.GetRolesAsync(rootUser))
            .ReturnsAsync(new List<string> { AppRoles.Root });

        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset-token");

        _userManagerMock.Setup(x => x.ResetPasswordAsync(user, "reset-token", It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
    }
}
