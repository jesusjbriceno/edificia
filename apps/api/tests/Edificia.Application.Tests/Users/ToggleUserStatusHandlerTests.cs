using Edificia.Application.Users;
using Edificia.Application.Users.Commands.ToggleUserStatus;
using Edificia.Domain.Constants;
using Edificia.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Edificia.Application.Tests.Users;

public class ToggleUserStatusHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ILogger<ToggleUserStatusHandler>> _loggerMock;
    private readonly ToggleUserStatusHandler _handler;

    public ToggleUserStatusHandlerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _loggerMock = new Mock<ILogger<ToggleUserStatusHandler>>();

        _handler = new ToggleUserStatusHandler(
            _userManagerMock.Object,
            _loggerMock.Object);
    }

    private static ApplicationUser CreateTestUser(bool isActive = true)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "user@edificia.dev",
            UserName = "user@edificia.dev",
            FullName = "Test User",
            IsActive = isActive
        };
    }

    [Fact]
    public async Task Handle_ShouldDeactivateUser_WhenActivateIsFalse()
    {
        // Arrange
        var user = CreateTestUser(isActive: true);
        var rootUserId = Guid.NewGuid();
        var rootUser = new ApplicationUser { Id = rootUserId };

        var command = new ToggleUserStatusCommand(user.Id, Activate: false, rootUserId);

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { AppRoles.Architect });

        _userManagerMock.Setup(x => x.FindByIdAsync(rootUserId.ToString()))
            .ReturnsAsync(rootUser);

        _userManagerMock.Setup(x => x.GetRolesAsync(rootUser))
            .ReturnsAsync(new List<string> { AppRoles.Root });

        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeFalse();
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldActivateUser_WhenActivateIsTrue()
    {
        // Arrange
        var user = CreateTestUser(isActive: false);
        var rootUserId = Guid.NewGuid();
        var rootUser = new ApplicationUser { Id = rootUserId };

        var command = new ToggleUserStatusCommand(user.Id, Activate: true, rootUserId);

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { AppRoles.Architect });

        _userManagerMock.Setup(x => x.FindByIdAsync(rootUserId.ToString()))
            .ReturnsAsync(rootUser);

        _userManagerMock.Setup(x => x.GetRolesAsync(rootUser))
            .ReturnsAsync(new List<string> { AppRoles.Root });

        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTogglingSelf()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ToggleUserStatusCommand(userId, Activate: false, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("CannotDeactivateSelf");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var command = new ToggleUserStatusCommand(Guid.NewGuid(), false, Guid.NewGuid());

        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAdminTriesToToggleAdmin()
    {
        // Arrange
        var targetUser = CreateTestUser();
        var adminUserId = Guid.NewGuid();
        var adminUser = new ApplicationUser { Id = adminUserId };

        var command = new ToggleUserStatusCommand(targetUser.Id, false, adminUserId);

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
}
