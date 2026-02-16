using Edificia.Domain.Entities;

namespace Edificia.Application.Interfaces;

/// <summary>
/// Service for generating JWT tokens.
/// Implemented in Infrastructure layer.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT access token for the given user with their roles.
    /// Embeds MustChangePassword as a claim when applicable.
    /// </summary>
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
}
