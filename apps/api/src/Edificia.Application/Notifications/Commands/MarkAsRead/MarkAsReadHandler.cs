using Edificia.Domain.Errors;
using Edificia.Infrastructure.Persistence;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Edificia.Application.Notifications.Commands.MarkAsRead;

internal sealed class MarkAsReadHandler : IRequestHandler<MarkAsReadCommand, Result>
{
    private readonly EdificiaDbContext _dbContext;

    public MarkAsReadHandler(EdificiaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken);

        if (notification is null)
        {
            return Result.Failure(Error.NotFound("Notification.NotFound", "La notificación no existe."));
        }

        notification.MarkAsRead();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
