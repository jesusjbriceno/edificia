namespace Edificia.Domain.Constants;

/// <summary>
/// Custom JWT claim types used in EDIFICIA tokens.
/// </summary>
public static class AppClaims
{
    /// <summary>
    /// Authentication method reference claim.
    /// Value "pwd_change_required" indicates the user must change password.
    /// </summary>
    public const string AuthMethodReference = "amr";

    /// <summary>Value indicating password change is required before full access.</summary>
    public const string PasswordChangeRequired = "pwd_change_required";

    /// <summary>Full name claim.</summary>
    public const string FullName = "full_name";

    /// <summary>Collegiate number claim (architects only).</summary>
    public const string CollegiateNumber = "collegiate_number";
}
