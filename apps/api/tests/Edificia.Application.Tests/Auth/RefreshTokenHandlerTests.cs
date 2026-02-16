using Edificia.Application.Auth.Commands.Login;
using Edificia.Application.Auth.Commands.RefreshToken;
using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Edificia.Application.Tests.Auth;

public class RefreshTokenHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IRefreshTokenSettings> _refreshTokenSettingsMock;
    private readonly Mock<ILogger<RefreshTokenHandler>> _loggerMock;
    private readonly RefreshTokenHandler _handler;

    public RefreshTokenHandlerTests()
    {
        _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();

        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _refreshTokenSettingsMock = new Mock<IRefreshTokenSettings>();
        _refreshTokenSettingsMock.Setup(x => x.ExpirationDays).Returns(7);
        _loggerMock = new Mock<ILogger<RefreshTokenHandler>>();

        _handler = new RefreshTokenHandler(
            _refreshTokenRepoMock.Object,
            _userManagerMock.Object,
            _jwtTokenServiceMock.Object,
            _refreshTokenSettingsMock.Object,
            _loggerMock.Object);
    }

    private static ApplicationUser CreateTestUser(Guid? id = null, bool isActive = true)
    {
        return new ApplicationUser
        {
            Id = id ?? Guid.NewGuid(),
            Email = "test@edificia.dev",
            UserName = "test@edificia.dev",
            FullName = "Test User",
            CollegiateNumber = "COL-001",
            MustChangePassword = false,
            IsActive = isActive,
            EmailConfirmed = true
        };
    }

    private static Domain.Entities.RefreshToken CreateActiveToken(
        Guid userId, int expirationDays = 7)
    {
        return new Domain.Entities.RefreshToken(userId, expirationDays);
    }

    private static Domain.Entities.RefreshToken CreateRevokedToken(Guid userId)
    {
        var token = new Domain.Entities.RefreshToken(userId, 7);
        token.Revoke();
        return token;
    }

    private static Domain.Entities.RefreshToken CreateExpiredToken(Guid userId)
    {
        var token = new Domain.Entities.RefreshToken(userId, 7);
        token.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        return token;
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenTokenIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        var existingToken = CreateActiveToken(userId);
        var command = new RefreshTokenCommand(existingToken.Token);

        _refreshTokenRepoMock
            .Setup(x => x.GetByTokenAsync(existingToken.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Architect" });

        _jwtTokenServiceMock
            .Setup(x => x.GenerateAccessToken(user, It.IsAny<IList<string>>()))
            .Returns("new-access-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-access-token");
        result.Value.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.Value.RefreshToken.Should().NotBe(existingToken.Token,
            "a new refresh token should be issued (rotation)");
        result.Value.User.Email.Should().Be("test@edificia.dev");
    }

    [Fact]
    public async Task Handle_ShouldRevokeOldToken_WhenRotating()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        var existingToken = CreateActiveToken(userId);
        var command = new RefreshTokenCommand(existingToken.Token);

        _refreshTokenRepoMock
            .Setup(x => x.GetByTokenAsync(existingToken.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Architect" });
        _jwtTokenServiceMock
            .Setup(x => x.GenerateAccessToken(user, It.IsAny<IList<string>>()))
            .Returns("token");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingToken.RevokedAt.Should().NotBeNull("old token must be revoked during rotation");
        existingToken.ReplacedByTokenId.Should().NotBeNull("old token must link to replacement");

        _refreshTokenRepoMock.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Once, "new token must be persisted");

        _refreshTokenRepoMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTokenNotFound()
    {
        // Arrange
        var command = new RefreshTokenCommand("non-existent-token");

        _refreshTokenRepoMock
            .Setup(x => x.GetByTokenAsync("non-existent-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.RefreshToken?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("InvalidRefreshToken");
    }

    [Fact]
    public async Task Handle_ShouldRevokeAllTokens_WhenRevokedTokenIsReused()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var revokedToken = CreateRevokedToken(userId);
        var command = new RefreshTokenCommand(revokedToken.Token);

        _refreshTokenRepoMock
            .Setup(x => x.GetByTokenAsync(revokedToken.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(revokedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("InvalidRefreshToken");

        _refreshTokenRepoMock.Verify(
            x => x.RevokeAllForUserAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once, "stolen token detection must revoke entire family");

        _refreshTokenRepoMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTokenIsExpired()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiredToken = CreateExpiredToken(userId);
        var command = new RefreshTokenCommand(expiredToken.Token);

        _refreshTokenRepoMock
            .Setup(x => x.GetByTokenAsync(expiredToken.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("RefreshTokenExpired");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsInactive()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activeToken = CreateActiveToken(userId);
        var inactiveUser = CreateTestUser(id: userId, isActive: false);
        var command = new RefreshTokenCommand(activeToken.Token);

        _refreshTokenRepoMock
            .Setup(x => x.GetByTokenAsync(activeToken.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeToken);

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(inactiveUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("AccountInactive");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activeToken = CreateActiveToken(userId);
        var command = new RefreshTokenCommand(activeToken.Token);

        _refreshTokenRepoMock
            .Setup(x => x.GetByTokenAsync(activeToken.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeToken);

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("AccountInactive");
    }

    [Fact]
    public async Task Handle_ShouldNotRevokeAll_WhenTokenIsExpiredButNotReused()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiredToken = CreateExpiredToken(userId);
        var command = new RefreshTokenCommand(expiredToken.Token);

        _refreshTokenRepoMock
            .Setup(x => x.GetByTokenAsync(expiredToken.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredToken);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — expired but not revoked should NOT trigger family revocation
        _refreshTokenRepoMock.Verify(
            x => x.RevokeAllForUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never, "expired-only token should not trigger stolen detection");
    }
}
