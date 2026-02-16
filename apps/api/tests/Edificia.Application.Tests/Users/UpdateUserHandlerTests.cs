using Edificia.Application.Users;
using Edificia.Application.Users.Commands.UpdateUser;
using Edificia.Domain.Constants;
using Edificia.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Edificia.Application.Tests.Users;

public class UpdateUserHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ILogger<UpdateUserHandler>> _loggerMock;
    private readonly UpdateUserHandler _handler;

    public UpdateUserHandlerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _loggerMock = new Mock<ILogger<UpdateUserHandler>>();

        _handler = new UpdateUserHandler(
            _userManagerMock.Object,
            _loggerMock.Object);
    }

    private static ApplicationUser CreateTestUser()
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "user@edificia.dev",
            UserName = "user@edificia.dev",
            FullName = "Original Name",
            CollegiateNumber = "COL-001",
            IsActive = true
        };
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserIsUpdatedSuccessfully()
    {
        // Arrange
        var user = CreateTestUser();
        var rootUserId = Guid.NewGuid();
        var rootUser = new ApplicationUser { Id = rootUserId, Email = "root@edificia.dev" };

        var command = new UpdateUserCommand(
            user.Id, "Updated Name", AppRoles.Architect, "COL-NEW", rootUserId);

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
        user.FullName.Should().Be("Updated Name");
        user.CollegiateNumber.Should().Be("COL-NEW");
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldChangeRole_WhenRoleDiffers()
    {
        // Arrange
        var user = CreateTestUser();
        var rootUserId = Guid.NewGuid();
        var rootUser = new ApplicationUser { Id = rootUserId };

        var command = new UpdateUserCommand(
            user.Id, "Test User", AppRoles.Collaborator, null, rootUserId);

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

        _userManagerMock.Setup(x => x.RemoveFromRoleAsync(user, AppRoles.Architect))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.AddToRoleAsync(user, AppRoles.Collaborator))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userManagerMock.Verify(x => x.RemoveFromRoleAsync(user, AppRoles.Architect), Times.Once);
        _userManagerMock.Verify(x => x.AddToRoleAsync(user, AppRoles.Collaborator), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotChangeRole_WhenRoleIsSame()
    {
        // Arrange
        var user = CreateTestUser();
        var rootUserId = Guid.NewGuid();
        var rootUser = new ApplicationUser { Id = rootUserId };

        var command = new UpdateUserCommand(
            user.Id, "Test User", AppRoles.Architect, null, rootUserId);

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
        _userManagerMock.Verify(x => x.RemoveFromRoleAsync(
            It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(x => x.AddToRoleAsync(
            It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var command = new UpdateUserCommand(
            Guid.NewGuid(), "Test User", AppRoles.Architect, null, Guid.NewGuid());

        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAdminTriesToModifyAdmin()
    {
        // Arrange
        var targetUser = CreateTestUser();
        var adminUserId = Guid.NewGuid();
        var adminUser = new ApplicationUser { Id = adminUserId };

        var command = new UpdateUserCommand(
            targetUser.Id, "Test", AppRoles.Architect, null, adminUserId);

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
    public async Task Handle_ShouldReturnFailure_WhenAdminTriesToAssignAdminRole()
    {
        // Arrange
        var targetUser = CreateTestUser();
        var adminUserId = Guid.NewGuid();
        var adminUser = new ApplicationUser { Id = adminUserId };

        var command = new UpdateUserCommand(
            targetUser.Id, "Test", AppRoles.Admin, null, adminUserId);

        _userManagerMock.Setup(x => x.FindByIdAsync(targetUser.Id.ToString()))
            .ReturnsAsync(targetUser);

        _userManagerMock.Setup(x => x.GetRolesAsync(targetUser))
            .ReturnsAsync(new List<string> { AppRoles.Architect });

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
