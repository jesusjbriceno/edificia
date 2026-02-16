using Microsoft.AspNetCore.Identity;

namespace Edificia.Domain.Entities;

/// <summary>
/// Represents an application user extending ASP.NET Core Identity.
/// Contains additional profile fields specific to EDIFICIA.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser()
    {
        if (Id == Guid.Empty)
            Id = Guid.NewGuid();
    }

    /// <summary>Nombre completo del usuario.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Número de colegiado (solo arquitectos).</summary>
    public string? CollegiateNumber { get; set; }

    /// <summary>
    /// Indica si el usuario debe cambiar su contraseña antes de operar.
    /// Se activa al crear el usuario Root por primera vez (seeding).
    /// </summary>
    public bool MustChangePassword { get; set; }

    /// <summary>Indica si la cuenta está activa.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Fecha de creación de la cuenta.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Última fecha de modificación.</summary>
    public DateTime? UpdatedAt { get; set; }
}
