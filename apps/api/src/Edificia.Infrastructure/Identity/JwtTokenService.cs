using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Edificia.Application.Interfaces;
using Edificia.Domain.Constants;
using Edificia.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Edificia.Infrastructure.Identity;

/// <summary>
/// Generates signed JWT access tokens with user claims and roles.
/// Includes the "pwd_change_required" AMR claim when MustChangePassword is true.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public string GenerateAccessToken(ApplicationUser user, IList<string> roles)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_settings.SecretKey));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(AppClaims.FullName, user.FullName)
        };

        // Add collegiate number if present
        if (!string.IsNullOrEmpty(user.CollegiateNumber))
        {
            claims.Add(new Claim(AppClaims.CollegiateNumber, user.CollegiateNumber));
        }

        // Add roles as claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // CRITICAL: Add password change required claim
        if (user.MustChangePassword)
        {
            claims.Add(new Claim(AppClaims.AuthMethodReference, AppClaims.PasswordChangeRequired));
        }

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
