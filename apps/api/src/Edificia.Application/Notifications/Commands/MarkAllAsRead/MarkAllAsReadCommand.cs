using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Notifications.Commands.MarkAllAsRead;

public sealed record MarkAllAsReadCommand(Guid UserId) : IRequest<Result>;
