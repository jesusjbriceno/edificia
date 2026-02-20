using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Notifications.Commands.MarkAsRead;

public sealed record MarkAsReadCommand(Guid NotificationId) : IRequest<Result>;
