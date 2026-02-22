namespace Edificia.Domain.Entities;

using Edificia.Domain.Primitives;

/// <summary>
/// Representa una notificación o alerta del sistema para un usuario específico.
/// </summary>
public sealed class Notification : AuditableEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public Guid UserId { get; private set; }

    // EF Core requiere constructor sin parámetros
    private Notification() { }

    /// <summary>
    /// Crea una nueva notificación.
    /// </summary>
    public static Notification Create(Guid userId, string title, string message)
    {
        return new Notification(Guid.NewGuid())
        {
            UserId = userId,
            Title = title,
            Message = message,
            IsRead = false
        };
    }

    private Notification(Guid id) : base(id) { }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
