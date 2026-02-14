namespace Edificia.Domain.Exceptions;

/// <summary>
/// Thrown when a domain entity is not found.
/// </summary>
public sealed class EntityNotFoundException : DomainException
{
    public string EntityName { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityName, object entityId)
        : base(
            "Domain.EntityNotFound",
            $"La entidad '{entityName}' con identificador '{entityId}' no fue encontrada.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}
