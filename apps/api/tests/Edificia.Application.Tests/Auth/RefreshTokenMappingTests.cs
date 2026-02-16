using Edificia.Application.Auth.Commands.RefreshToken;
using Edificia.Application.Auth.Commands.RevokeToken;
using Edificia.Application.Auth.DTOs;
using FluentAssertions;

namespace Edificia.Application.Tests.Auth;

public class RefreshTokenMappingTests
{
    [Fact]
    public void RefreshTokenRequest_ShouldMapToCommand()
    {
        // Arrange
        var request = new RefreshTokenRequest("my-refresh-token-value");

        // Act
        var command = (RefreshTokenCommand)request;

        // Assert
        command.Token.Should().Be("my-refresh-token-value");
    }

    [Fact]
    public void RevokeTokenRequest_ShouldMapToCommand()
    {
        // Arrange
        var request = new RevokeTokenRequest("token-to-revoke");

        // Act
        var command = (RevokeTokenCommand)request;

        // Assert
        command.Token.Should().Be("token-to-revoke");
    }
}
