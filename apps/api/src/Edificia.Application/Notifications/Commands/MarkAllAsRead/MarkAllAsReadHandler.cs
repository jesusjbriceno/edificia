using Edificia.Infrastructure.Persistence;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Edificia.Application.Notifications.Commands.MarkAllAsRead;

internal sealed class MarkAllAsReadHandler : IRequestHandler<MarkAllAsReadCommand, Result>
{
    private readonly EdificiaDbContext _dbContext;

    public MarkAllAsReadHandler(EdificiaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        var unreadNotifications = await _dbContext.Notifications
            .Where(n => n.UserId == request.UserId && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in unreadNotifications)
        {
            notification.MarkAsRead();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
