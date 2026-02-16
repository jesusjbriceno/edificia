using Dapper;
using Edificia.Application.Interfaces;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Queries.GetProjectById;

/// <summary>
/// Handles GetProjectByIdQuery using Dapper for optimized reads.
/// </summary>
public sealed class GetProjectByIdHandler : IRequestHandler<GetProjectByIdQuery, Result<ProjectResponse>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetProjectByIdHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<ProjectResponse>> Handle(
        GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var project = await connection.QuerySingleOrDefaultAsync<ProjectResponse>(
            ProjectSqlQueries.GetById, new { request.Id });

        return project is null
            ? Result.Failure<ProjectResponse>(
                Error.NotFound("Project", $"No se encontró el proyecto con ID '{request.Id}'."))
            : project;
    }
}
