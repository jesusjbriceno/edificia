using Edificia.Application.Interfaces;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Notifications.Commands.MarkAllAsRead;

internal sealed class MarkAllAsReadHandler : IRequestHandler<MarkAllAsReadCommand, Result>
{
    private readonly INotificationRepository _notificationRepository;

    public MarkAllAsReadHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Result> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        var unreadNotifications = await _notificationRepository
            .FindAsync(n => n.UserId == request.UserId && !n.IsRead, cancellationToken);

        foreach (var notification in unreadNotifications)
        {
            notification.MarkAsRead();
        }

        await _notificationRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
