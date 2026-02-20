using Edificia.Domain.Enums;
using Edificia.Domain.Primitives;

namespace Edificia.Domain.Entities;

/// <summary>
/// Represents the membership relationship between a user and a project.
/// Composite key: (ProjectId, UserId). Each user can have one role per project.
/// </summary>
public sealed class ProjectMember : AuditableEntity
{
    /// <summary>ID del proyecto al que pertenece el miembro.</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>ID del usuario miembro.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Rol del miembro dentro del proyecto.</summary>
    public ProjectMemberRole Role { get; private set; }

    /// <summary>Navegación al proyecto.</summary>
    public Project Project { get; private set; } = null!;

    /// <summary>Navegación al usuario.</summary>
    public ApplicationUser User { get; private set; } = null!;

    // EF Core requires parameterless constructor
    private ProjectMember() { }

    /// <summary>
    /// Creates a new project membership.
    /// </summary>
    public static ProjectMember Create(Guid projectId, Guid userId, ProjectMemberRole role)
    {
        return new ProjectMember(Guid.NewGuid())
        {
            ProjectId = projectId,
            UserId = userId,
            Role = role
        };
    }

    private ProjectMember(Guid id) : base(id) { }

    /// <summary>Updates the member's role within the project.</summary>
    public void ChangeRole(ProjectMemberRole newRole)
    {
        Role = newRole;
    }
}
