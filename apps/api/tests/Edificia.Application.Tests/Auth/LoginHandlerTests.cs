using Edificia.Application.Auth.Commands.Login;
using Edificia.Application.Auth.DTOs;
using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Edificia.Application.Tests.Auth;

public class LoginHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<ILogger<LoginHandler>> _loggerMock;
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _loggerMock = new Mock<ILogger<LoginHandler>>();

        _handler = new LoginHandler(
            _userManagerMock.Object,
            _jwtTokenServiceMock.Object,
            _loggerMock.Object);
    }

    private static ApplicationUser CreateTestUser(
        bool mustChangePassword = false,
        bool isActive = true)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@edificia.dev",
            UserName = "test@edificia.dev",
            FullName = "Test User",
            CollegiateNumber = "COL-001",
            MustChangePassword = mustChangePassword,
            IsActive = isActive,
            EmailConfirmed = true
        };
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        // Arrange
        var user = CreateTestUser();
        var command = new LoginCommand("test@edificia.dev", "ValidPass123!");

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.IsLockedOutAsync(user))
            .ReturnsAsync(false);

        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, command.Password))
            .ReturnsAsync(true);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Architect" });

        _jwtTokenServiceMock.Setup(x => x.GenerateAccessToken(user, It.IsAny<IList<string>>()))
            .Returns("test-jwt-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("test-jwt-token");
        result.Value.MustChangePassword.Should().BeFalse();
        result.Value.User.Email.Should().Be("test@edificia.dev");
        result.Value.User.FullName.Should().Be("Test User");
        result.Value.User.Roles.Should().Contain("Architect");
    }

    [Fact]
    public async Task Handle_ShouldReturnMustChangePasswordTrue_WhenFlagIsSet()
    {
        // Arrange
        var user = CreateTestUser(mustChangePassword: true);
        var command = new LoginCommand("test@edificia.dev", "ValidPass123!");

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.IsLockedOutAsync(user))
            .ReturnsAsync(false);

        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, command.Password))
            .ReturnsAsync(true);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Root" });

        _jwtTokenServiceMock.Setup(x => x.GenerateAccessToken(user, It.IsAny<IList<string>>()))
            .Returns("restricted-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.MustChangePassword.Should().BeTrue();
        result.Value.AccessToken.Should().Be("restricted-token");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var command = new LoginCommand("nonexistent@edificia.dev", "Password123!");

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("InvalidCredentials");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAccountIsInactive()
    {
        // Arrange
        var user = CreateTestUser(isActive: false);
        var command = new LoginCommand("test@edificia.dev", "ValidPass123!");

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("AccountInactive");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAccountIsLockedOut()
    {
        // Arrange
        var user = CreateTestUser();
        var command = new LoginCommand("test@edificia.dev", "ValidPass123!");

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.IsLockedOutAsync(user))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("AccountLockedOut");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPasswordIsWrong()
    {
        // Arrange
        var user = CreateTestUser();
        var command = new LoginCommand("test@edificia.dev", "WrongPass!");

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.IsLockedOutAsync(user))
            .ReturnsAsync(false);

        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, command.Password))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("InvalidCredentials");
    }

    [Fact]
    public async Task Handle_ShouldIncludeCollegiateNumber_WhenPresent()
    {
        // Arrange
        var user = CreateTestUser();
        var command = new LoginCommand("test@edificia.dev", "ValidPass123!");

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.IsLockedOutAsync(user))
            .ReturnsAsync(false);

        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, command.Password))
            .ReturnsAsync(true);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Architect" });

        _jwtTokenServiceMock.Setup(x => x.GenerateAccessToken(user, It.IsAny<IList<string>>()))
            .Returns("token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Value.User.CollegiateNumber.Should().Be("COL-001");
    }
}
