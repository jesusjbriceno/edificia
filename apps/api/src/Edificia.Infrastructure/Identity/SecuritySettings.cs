namespace Edificia.Infrastructure.Identity;

/// <summary>
/// Settings for the initial Root user seeding.
/// Mapped from "Security" configuration section.
/// </summary>
public sealed class SecuritySettings
{
    public const string SectionName = "Security";

    /// <summary>Email of the root administrator account.</summary>
    public string RootEmail { get; set; } = "admin@edificia.dev";

    /// <summary>Initial password for the root account. Must be changed on first login.</summary>
    public string RootInitialPassword { get; set; } = "ChangeMe123!";
}
