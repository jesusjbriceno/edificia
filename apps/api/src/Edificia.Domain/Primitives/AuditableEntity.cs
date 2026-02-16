namespace Edificia.Domain.Primitives;

/// <summary>
/// Entity with audit timestamps (created_at, updated_at in PostgreSQL).
/// </summary>
public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    protected AuditableEntity(Guid id) : base(id) { }

    protected AuditableEntity() { }
}
