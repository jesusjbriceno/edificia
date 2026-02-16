namespace Edificia.Domain.Constants;

/// <summary>
/// Authorization policy names used across the application.
/// </summary>
public static class AppPolicies
{
    /// <summary>
    /// Requires an active user who does NOT need to change password.
    /// Applied globally to all endpoints except /auth/change-password.
    /// </summary>
    public const string ActiveUser = "ActiveUser";

    /// <summary>Requires Root role.</summary>
    public const string RequireRoot = "RequireRoot";

    /// <summary>Requires Admin or Root role.</summary>
    public const string RequireAdmin = "RequireAdmin";

    /// <summary>Requires Architect, Admin, or Root role.</summary>
    public const string RequireArchitect = "RequireArchitect";
}
