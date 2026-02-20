using Dapper;
using Edificia.Application.Interfaces;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Notifications.Queries.GetNotifications;

internal sealed class GetNotificationsHandler 
    : IRequestHandler<GetNotificationsQuery, Result<IReadOnlyList<NotificationResponse>>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetNotificationsHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<IReadOnlyList<NotificationResponse>>> Handle(
        GetNotificationsQuery request, 
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                id AS Id,
                title AS Title,
                message AS Message,
                is_read AS IsRead,
                created_at AS CreatedAt
            FROM notifications
            WHERE user_id = @UserId
            ORDER BY created_at DESC
            LIMIT 50";

        var notifications = await connection.QueryAsync<NotificationResponse>(
            sql, 
            new { request.UserId });

        IReadOnlyList<NotificationResponse> result = notifications.ToList().AsReadOnly();
        return Result<IReadOnlyList<NotificationResponse>>.Success(result);
    }
}
