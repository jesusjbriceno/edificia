using Edificia.Application.Interfaces;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Notifications.Commands.MarkAsRead;

internal sealed class MarkAsReadHandler : IRequestHandler<MarkAsReadCommand, Result>
{
    private readonly INotificationRepository _notificationRepository;

    public MarkAsReadHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Result> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(request.NotificationId, cancellationToken);

        if (notification is null)
        {
            return Result.Failure(Error.NotFound("Notification.NotFound", "La notificación no existe."));
        }

        notification.MarkAsRead();

        await _notificationRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
