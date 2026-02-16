using Edificia.Application.Interfaces;
using Edificia.Application.Users;
using Edificia.Application.Users.Commands.CreateUser;
using Edificia.Domain.Constants;
using Edificia.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Edificia.Application.Tests.Users;

public class CreateUserHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<CreateUserHandler>> _loggerMock;
    private readonly CreateUserHandler _handler;

    public CreateUserHandlerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<CreateUserHandler>>();

        _handler = new CreateUserHandler(
            _userManagerMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object);
    }

    private static ApplicationUser CreateRootUser()
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "root@edificia.dev",
            UserName = "root@edificia.dev",
            FullName = "Root Admin",
            IsActive = true
        };
    }

    private static ApplicationUser CreateAdminUser()
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "admin@edificia.dev",
            UserName = "admin@edificia.dev",
            FullName = "Admin User",
            IsActive = true
        };
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserIsCreatedSuccessfully()
    {
        // Arrange
        var rootUserId = Guid.NewGuid();
        var command = new CreateUserCommand(
            "new@edificia.dev", "New User", AppRoles.Architect, "COL-001", rootUserId);

        var rootUser = CreateRootUser();
        rootUser.Id = rootUserId;

        _userManagerMock.Setup(x => x.FindByIdAsync(rootUserId.ToString()))
            .ReturnsAsync(rootUser);

        _userManagerMock.Setup(x => x.GetRolesAsync(rootUser))
            .ReturnsAsync(new List<string> { AppRoles.Root });

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), AppRoles.Architect))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _userManagerMock.Verify(x => x.CreateAsync(
            It.Is<ApplicationUser>(u =>
                u.Email == "new@edificia.dev" &&
                u.FullName == "New User" &&
                u.MustChangePassword == true &&
                u.IsActive == true),
            It.IsAny<string>()), Times.Once);

        _userManagerMock.Verify(x => x.AddToRoleAsync(
            It.IsAny<ApplicationUser>(), AppRoles.Architect), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSendWelcomeEmail_WhenUserIsCreated()
    {
        // Arrange
        var rootUserId = Guid.NewGuid();
        var command = new CreateUserCommand(
            "new@edificia.dev", "New User", AppRoles.Architect, null, rootUserId);

        var rootUser = CreateRootUser();
        rootUser.Id = rootUserId;

        _userManagerMock.Setup(x => x.FindByIdAsync(rootUserId.ToString()))
            .ReturnsAsync(rootUser);

        _userManagerMock.Setup(x => x.GetRolesAsync(rootUser))
            .ReturnsAsync(new List<string> { AppRoles.Root });

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(x => x.SendAsync(
            "new@edificia.dev",
            It.Is<string>(s => s.Contains("Bienvenido")),
            It.Is<string>(body => body.Contains("New User")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        // Arrange
        var rootUserId = Guid.NewGuid();
        var command = new CreateUserCommand(
            "existing@edificia.dev", "Test User", AppRoles.Architect, null, rootUserId);

        var rootUser = CreateRootUser();
        rootUser.Id = rootUserId;

        _userManagerMock.Setup(x => x.FindByIdAsync(rootUserId.ToString()))
            .ReturnsAsync(rootUser);

        _userManagerMock.Setup(x => x.GetRolesAsync(rootUser))
            .ReturnsAsync(new List<string> { AppRoles.Root });

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(new ApplicationUser { Email = "existing@edificia.dev" });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("EmailAlreadyExists");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAdminTriesToCreateAdmin()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var command = new CreateUserCommand(
            "newadmin@edificia.dev", "New Admin", AppRoles.Admin, null, adminUserId);

        var adminUser = CreateAdminUser();
        adminUser.Id = adminUserId;

        _userManagerMock.Setup(x => x.FindByIdAsync(adminUserId.ToString()))
            .ReturnsAsync(adminUser);

        _userManagerMock.Setup(x => x.GetRolesAsync(adminUser))
            .ReturnsAsync(new List<string> { AppRoles.Admin });

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("CannotModifyHigherRole");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCreateFails()
    {
        // Arrange
        var rootUserId = Guid.NewGuid();
        var command = new CreateUserCommand(
            "new@edificia.dev", "Test User", AppRoles.Architect, null, rootUserId);

        var rootUser = CreateRootUser();
        rootUser.Id = rootUserId;

        _userManagerMock.Setup(x => x.FindByIdAsync(rootUserId.ToString()))
            .ReturnsAsync(rootUser);

        _userManagerMock.Setup(x => x.GetRolesAsync(rootUser))
            .ReturnsAsync(new List<string> { AppRoles.Root });

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Err", Description = "Failed" }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("CreationFailed");
    }

    [Fact]
    public async Task Handle_ShouldRollback_WhenRoleAssignmentFails()
    {
        // Arrange
        var rootUserId = Guid.NewGuid();
        var command = new CreateUserCommand(
            "new@edificia.dev", "Test User", AppRoles.Architect, null, rootUserId);

        var rootUser = CreateRootUser();
        rootUser.Id = rootUserId;

        _userManagerMock.Setup(x => x.FindByIdAsync(rootUserId.ToString()))
            .ReturnsAsync(rootUser);

        _userManagerMock.Setup(x => x.GetRolesAsync(rootUser))
            .ReturnsAsync(new List<string> { AppRoles.Root });

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Err", Description = "Failed" }));

        _userManagerMock.Setup(x => x.DeleteAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("RoleChangeFailed");
        _userManagerMock.Verify(x => x.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_EvenWhenEmailFails()
    {
        // Arrange
        var rootUserId = Guid.NewGuid();
        var command = new CreateUserCommand(
            "new@edificia.dev", "Test User", AppRoles.Architect, null, rootUserId);

        var rootUser = CreateRootUser();
        rootUser.Id = rootUserId;

        _userManagerMock.Setup(x => x.FindByIdAsync(rootUserId.ToString()))
            .ReturnsAsync(rootUser);

        _userManagerMock.Setup(x => x.GetRolesAsync(rootUser))
            .ReturnsAsync(new List<string> { AppRoles.Root });

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _emailServiceMock.Setup(x => x.SendAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SMTP failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
