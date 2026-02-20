using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Notifications.Queries.GetNotifications;

public sealed record GetNotificationsQuery(Guid UserId) : IRequest<Result<IReadOnlyList<NotificationResponse>>>;

public sealed record NotificationResponse(
    Guid Id,
    string Title,
    string Message,
    bool IsRead,
    DateTime CreatedAt);
