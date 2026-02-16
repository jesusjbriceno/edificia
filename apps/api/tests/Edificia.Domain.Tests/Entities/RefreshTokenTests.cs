using Edificia.Domain.Entities;
using FluentAssertions;

namespace Edificia.Domain.Tests.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void Constructor_ShouldGenerateToken_WithValidBase64()
    {
        // Act
        var refreshToken = new RefreshToken(Guid.NewGuid(), expirationDays: 7);

        // Assert
        refreshToken.Token.Should().NotBeNullOrWhiteSpace();
        var bytes = Convert.FromBase64String(refreshToken.Token);
        bytes.Should().HaveCount(64, "token should be 64 bytes encoded as Base64");
    }

    [Fact]
    public void Constructor_ShouldSetUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var refreshToken = new RefreshToken(userId, expirationDays: 7);

        // Assert
        refreshToken.UserId.Should().Be(userId);
    }

    [Fact]
    public void Constructor_ShouldSetExpiresAt_BasedOnExpirationDays()
    {
        // Arrange
        var before = DateTime.UtcNow.AddDays(7);

        // Act
        var refreshToken = new RefreshToken(Guid.NewGuid(), expirationDays: 7);

        var after = DateTime.UtcNow.AddDays(7);

        // Assert
        refreshToken.ExpiresAt.Should().BeOnOrAfter(before.AddSeconds(-1));
        refreshToken.ExpiresAt.Should().BeOnOrBefore(after.AddSeconds(1));
    }

    [Fact]
    public void Constructor_ShouldSetCreatedAt_ToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var refreshToken = new RefreshToken(Guid.NewGuid(), expirationDays: 7);

        // Assert
        refreshToken.CreatedAt.Should().BeOnOrAfter(before.AddSeconds(-1));
        refreshToken.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void Constructor_ShouldSetUniqueId()
    {
        // Act
        var refreshToken = new RefreshToken(Guid.NewGuid(), expirationDays: 7);

        // Assert
        refreshToken.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueTokens()
    {
        // Act
        var token1 = new RefreshToken(Guid.NewGuid(), expirationDays: 7);
        var token2 = new RefreshToken(Guid.NewGuid(), expirationDays: 7);

        // Assert
        token1.Token.Should().NotBe(token2.Token);
    }

    [Fact]
    public void IsActive_ShouldReturnTrue_WhenNotRevokedAndNotExpired()
    {
        // Arrange
        var refreshToken = new RefreshToken(Guid.NewGuid(), expirationDays: 7);

        // Assert
        refreshToken.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenRevoked()
    {
        // Arrange
        var refreshToken = new RefreshToken(Guid.NewGuid(), expirationDays: 7);

        // Act
        refreshToken.Revoke();

        // Assert
        refreshToken.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenExpired()
    {
        // Arrange
        var refreshToken = new RefreshToken(Guid.NewGuid(), expirationDays: 7);
        // Force expiration
        refreshToken.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);

        // Assert
        refreshToken.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Revoke_ShouldSetRevokedAt()
    {
        // Arrange
        var refreshToken = new RefreshToken(Guid.NewGuid(), expirationDays: 7);
        var before = DateTime.UtcNow;

        // Act
        refreshToken.Revoke();

        // Assert
        refreshToken.RevokedAt.Should().NotBeNull();
        refreshToken.RevokedAt!.Value.Should().BeOnOrAfter(before.AddSeconds(-1));
    }

    [Fact]
    public void Revoke_ShouldSetReplacedByTokenId_WhenProvided()
    {
        // Arrange
        var refreshToken = new RefreshToken(Guid.NewGuid(), expirationDays: 7);
        var replacementId = Guid.NewGuid();

        // Act
        refreshToken.Revoke(replacedByTokenId: replacementId);

        // Assert
        refreshToken.ReplacedByTokenId.Should().Be(replacementId);
        refreshToken.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public void Revoke_ShouldLeaveReplacedByTokenIdNull_WhenNotProvided()
    {
        // Arrange
        var refreshToken = new RefreshToken(Guid.NewGuid(), expirationDays: 7);

        // Act
        refreshToken.Revoke();

        // Assert
        refreshToken.ReplacedByTokenId.Should().BeNull();
        refreshToken.RevokedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(14)]
    [InlineData(30)]
    public void Constructor_ShouldRespectDifferentExpirationDays(int days)
    {
        // Arrange
        var before = DateTime.UtcNow.AddDays(days);

        // Act
        var refreshToken = new RefreshToken(Guid.NewGuid(), expirationDays: days);

        // Assert
        refreshToken.ExpiresAt.Should().BeCloseTo(before, TimeSpan.FromSeconds(2));
    }
}
