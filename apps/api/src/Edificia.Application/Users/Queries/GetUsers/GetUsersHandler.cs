using System.Text;
using Dapper;
using Edificia.Application.Common;
using Edificia.Application.Interfaces;
using Edificia.Application.Users.DTOs;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Users.Queries.GetUsers;

/// <summary>
/// Handles GetUsersQuery using Dapper for optimized paginated reads.
/// </summary>
public sealed class GetUsersHandler
    : IRequestHandler<GetUsersQuery, Result<PagedResponse<UserResponse>>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetUsersHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<PagedResponse<UserResponse>>> Handle(
        GetUsersQuery request, CancellationToken cancellationToken)
    {
        var whereClause = new StringBuilder();
        var parameters = new DynamicParameters();
        var conditions = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            conditions.Add("r.name = @Role");
            parameters.Add("Role", request.Role);
        }

        if (request.IsActive.HasValue)
        {
            conditions.Add("u.is_active = @IsActive");
            parameters.Add("IsActive", request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            conditions.Add("(u.full_name ILIKE @Search OR u.email ILIKE @Search)");
            parameters.Add("Search", $"%{request.Search}%");
        }

        if (conditions.Count > 0)
        {
            whereClause.Append(" WHERE ");
            whereClause.Append(string.Join(" AND ", conditions));
        }

        var countSql = UserSqlQueries.Count(whereClause.ToString());

        var offset = (request.Page - 1) * request.PageSize;
        parameters.Add("Limit", request.PageSize);
        parameters.Add("Offset", offset);

        var dataSql = UserSqlQueries.GetPaged(whereClause.ToString());

        using var connection = _connectionFactory.CreateConnection();

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var items = await connection.QueryAsync<UserResponse>(dataSql, parameters);

        var pagedResponse = new PagedResponse<UserResponse>(
            Items: items.ToList().AsReadOnly(),
            TotalCount: totalCount,
            Page: request.Page,
            PageSize: request.PageSize);

        return Result<PagedResponse<UserResponse>>.Success(pagedResponse);
    }
}
