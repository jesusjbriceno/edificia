namespace Edificia.Domain.Constants;

/// <summary>
/// Application role constants for RBAC.
/// Matches the roles seeded in IdentityDataInitializer.
/// </summary>
public static class AppRoles
{
    /// <summary>Super administrador del sistema. Acceso total.</summary>
    public const string Root = "Root";

    /// <summary>Administrador de organización. Gestiona usuarios y proyectos.</summary>
    public const string Admin = "Admin";

    /// <summary>Arquitecto. Crea y edita proyectos, usa IA.</summary>
    public const string Architect = "Architect";

    /// <summary>Colaborador. Acceso de solo lectura o edición limitada.</summary>
    public const string Collaborator = "Collaborator";

    /// <summary>All roles for iteration/seeding.</summary>
    public static readonly string[] All = [Root, Admin, Architect, Collaborator];
}
