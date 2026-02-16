using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Edificia.Domain.Constants;
using Edificia.Domain.Entities;
using Edificia.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace Edificia.Application.Tests.Auth;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _service;
    private readonly JwtSettings _settings;

    public JwtTokenServiceTests()
    {
        _settings = new JwtSettings
        {
            SecretKey = "ThisIsAVerySecretKeyForTestingPurposesOnly123!",
            Issuer = "https://test-api.edificia.dev",
            Audience = "https://test.edificia.dev",
            ExpirationMinutes = 30
        };

        _service = new JwtTokenService(Options.Create(_settings));
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidJwt()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@edificia.dev",
            FullName = "Test User",
            MustChangePassword = false
        };
        var roles = new List<string> { "Architect" };

        // Act
        var token = _service.GenerateAccessToken(user, roles);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        jwt.Issuer.Should().Be(_settings.Issuer);
        jwt.Audiences.Should().Contain(_settings.Audience);
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeUserClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "architect@edificia.dev",
            FullName = "Architect User",
            CollegiateNumber = "COL-999",
            MustChangePassword = false
        };
        var roles = new List<string> { "Architect" };

        // Act
        var token = _service.GenerateAccessToken(user, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Subject.Should().Be(userId.ToString());
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "architect@edificia.dev");
        jwt.Claims.Should().Contain(c => c.Type == AppClaims.FullName && c.Value == "Architect User");
        jwt.Claims.Should().Contain(c => c.Type == AppClaims.CollegiateNumber && c.Value == "COL-999");
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeRoleClaims()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "admin@edificia.dev",
            FullName = "Admin User",
            MustChangePassword = false
        };
        var roles = new List<string> { "Root", "Admin" };

        // Act
        var token = _service.GenerateAccessToken(user, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var roleClaims = jwt.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
            .Select(c => c.Value)
            .ToList();

        roleClaims.Should().Contain("Root");
        roleClaims.Should().Contain("Admin");
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludePasswordChangeRequiredClaim_WhenFlagIsTrue()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "root@edificia.dev",
            FullName = "Root User",
            MustChangePassword = true
        };
        var roles = new List<string> { "Root" };

        // Act
        var token = _service.GenerateAccessToken(user, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c =>
            c.Type == AppClaims.AuthMethodReference &&
            c.Value == AppClaims.PasswordChangeRequired);
    }

    [Fact]
    public void GenerateAccessToken_ShouldNotIncludePasswordChangeClaim_WhenFlagIsFalse()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "user@edificia.dev",
            FullName = "Normal User",
            MustChangePassword = false
        };
        var roles = new List<string> { "Architect" };

        // Act
        var token = _service.GenerateAccessToken(user, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().NotContain(c => c.Type == AppClaims.AuthMethodReference);
    }

    [Fact]
    public void GenerateAccessToken_ShouldNotIncludeCollegiateNumber_WhenNull()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "admin@edificia.dev",
            FullName = "Admin User",
            CollegiateNumber = null,
            MustChangePassword = false
        };
        var roles = new List<string> { "Admin" };

        // Act
        var token = _service.GenerateAccessToken(user, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().NotContain(c => c.Type == AppClaims.CollegiateNumber);
    }

    [Fact]
    public void GenerateAccessToken_ShouldSetCorrectExpiration()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@edificia.dev",
            FullName = "Test",
            MustChangePassword = false
        };
        var roles = new List<string> { "Architect" };

        // Act
        var beforeGeneration = DateTime.UtcNow;
        var token = _service.GenerateAccessToken(user, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.ValidTo.Should().BeCloseTo(
            beforeGeneration.AddMinutes(_settings.ExpirationMinutes),
            precision: TimeSpan.FromSeconds(5));
    }
}
