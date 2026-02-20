namespace Edificia.Domain.Enums;

/// <summary>
/// Rol de un miembro dentro de un proyecto.
/// Determina el nivel de acceso al proyecto.
/// </summary>
public enum ProjectMemberRole
{
    /// <summary>Propietario del proyecto — control total.</summary>
    Owner = 0,

    /// <summary>Editor — puede modificar contenido del proyecto.</summary>
    Editor = 1,

    /// <summary>Visor — solo lectura del proyecto.</summary>
    Viewer = 2
}
