using System.Text;
using Dapper;
using Edificia.Application.Common;
using Edificia.Application.Interfaces;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Queries.GetProjects;

/// <summary>
/// Handles GetProjectsQuery using Dapper for optimized paginated reads.
/// </summary>
public sealed class GetProjectsHandler
    : IRequestHandler<GetProjectsQuery, Result<PagedResponse<ProjectResponse>>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetProjectsHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<PagedResponse<ProjectResponse>>> Handle(
        GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var whereClause = new StringBuilder();
        var parameters = new DynamicParameters();
        var conditions = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            conditions.Add("status = @Status");
            parameters.Add("Status", request.Status);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            conditions.Add("(title ILIKE @Search OR description ILIKE @Search)");
            parameters.Add("Search", $"%{request.Search}%");
        }

        if (conditions.Count > 0)
        {
            whereClause.Append(" WHERE ");
            whereClause.Append(string.Join(" AND ", conditions));
        }

        var countSql = ProjectSqlQueries.Count(whereClause.ToString());

        var offset = (request.Page - 1) * request.PageSize;
        parameters.Add("Limit", request.PageSize);
        parameters.Add("Offset", offset);

        var dataSql = ProjectSqlQueries.GetPaged(whereClause.ToString());

        using var connection = _connectionFactory.CreateConnection();

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var items = await connection.QueryAsync<ProjectResponse>(dataSql, parameters);

        var pagedResponse = new PagedResponse<ProjectResponse>(
            Items: items.ToList().AsReadOnly(),
            TotalCount: totalCount,
            Page: request.Page,
            PageSize: request.PageSize);

        return Result<PagedResponse<ProjectResponse>>.Success(pagedResponse);
    }
}
