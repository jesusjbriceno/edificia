using Dapper;
using Edificia.Application.Interfaces;
using Edificia.Application.Users.DTOs;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Users.Queries.GetUserById;

/// <summary>
/// Handles GetUserByIdQuery using Dapper for optimized reads.
/// </summary>
public sealed class GetUserByIdHandler
    : IRequestHandler<GetUserByIdQuery, Result<UserResponse>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetUserByIdHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<UserResponse>> Handle(
        GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var user = await connection.QueryFirstOrDefaultAsync<UserResponse>(
            UserSqlQueries.GetById, new { Id = request.Id });

        if (user is null)
            return Result.Failure<UserResponse>(UserErrors.UserNotFound);

        return Result.Success(user);
    }
}
