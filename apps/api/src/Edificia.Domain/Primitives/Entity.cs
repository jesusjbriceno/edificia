namespace Edificia.Domain.Primitives;

/// <summary>
/// Base class for all domain entities with a GUID identifier.
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; protected init; }

    protected Entity(Guid id)
    {
        Id = id;
    }

    // EF Core requires a parameterless constructor
    protected Entity() { }

    public override bool Equals(object? obj)
        => obj is Entity entity && Equals(entity);

    public bool Equals(Entity? other)
        => other is not null && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity? left, Entity? right)
        => left is not null ? left.Equals(right) : right is null;

    public static bool operator !=(Entity? left, Entity? right)
        => !(left == right);
}
