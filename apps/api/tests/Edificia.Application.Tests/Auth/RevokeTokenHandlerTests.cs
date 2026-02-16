using Edificia.Application.Auth.Commands.Login;
using Edificia.Application.Auth.Commands.RevokeToken;
using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Edificia.Application.Tests.Auth;

public class RevokeTokenHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
    private readonly Mock<ILogger<RevokeTokenHandler>> _loggerMock;
    private readonly RevokeTokenHandler _handler;

    public RevokeTokenHandlerTests()
    {
        _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
        _loggerMock = new Mock<ILogger<RevokeTokenHandler>>();

        _handler = new RevokeTokenHandler(
            _refreshTokenRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenTokenIsRevoked()
    {
        // Arrange
        var activeToken = new RefreshToken(Guid.NewGuid(), expirationDays: 7);
        var command = new RevokeTokenCommand(activeToken.Token);

        _refreshTokenRepoMock
            .Setup(x => x.GetByTokenAsync(activeToken.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        activeToken.RevokedAt.Should().NotBeNull("token should be marked as revoked");

        _refreshTokenRepoMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTokenNotFound()
    {
        // Arrange
        var command = new RevokeTokenCommand("non-existent-token");

        _refreshTokenRepoMock
            .Setup(x => x.GetByTokenAsync("non-existent-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("InvalidRefreshToken");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenTokenIsAlreadyRevoked()
    {
        // Arrange — token already revoked (idempotent behavior)
        var revokedToken = new RefreshToken(Guid.NewGuid(), expirationDays: 7);
        revokedToken.Revoke();
        var command = new RevokeTokenCommand(revokedToken.Token);

        _refreshTokenRepoMock
            .Setup(x => x.GetByTokenAsync(revokedToken.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(revokedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue("revocation should be idempotent");

        _refreshTokenRepoMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never, "no save needed when token is already revoked");
    }

    [Fact]
    public async Task Handle_ShouldNotCallSave_WhenTokenNotFound()
    {
        // Arrange
        var command = new RevokeTokenCommand("missing-token");

        _refreshTokenRepoMock
            .Setup(x => x.GetByTokenAsync("missing-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _refreshTokenRepoMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
