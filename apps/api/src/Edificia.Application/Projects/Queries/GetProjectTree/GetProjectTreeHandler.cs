using Dapper;
using Edificia.Application.Interfaces;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Queries.GetProjectTree;

/// <summary>
/// Handles GetProjectTreeQuery using Dapper for direct JSONB read.
/// Returns the content tree along with intervention metadata for client-side filtering.
/// </summary>
public sealed class GetProjectTreeHandler : IRequestHandler<GetProjectTreeQuery, Result<ContentTreeResponse>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetProjectTreeHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<ContentTreeResponse>> Handle(
        GetProjectTreeQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var result = await connection.QuerySingleOrDefaultAsync<ContentTreeResponse>(
            ProjectSqlQueries.GetTree, new { request.ProjectId });

        return result is null
            ? Result.Failure<ContentTreeResponse>(
                Error.NotFound("Project", $"No se encontró el proyecto con ID '{request.ProjectId}'."))
            : result;
    }
}
