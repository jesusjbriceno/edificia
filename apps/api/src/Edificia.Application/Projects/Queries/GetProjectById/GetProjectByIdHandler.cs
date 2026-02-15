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
        const string sql = """
            SELECT
                id              AS Id,
                title           AS Title,
                description     AS Description,
                address         AS Address,
                intervention_type AS InterventionType,
                is_loe_required AS IsLoeRequired,
                cadastral_reference AS CadastralReference,
                local_regulations AS LocalRegulations,
                status          AS Status,
                created_at      AS CreatedAt,
                updated_at      AS UpdatedAt
            FROM projects
            WHERE id = @Id
            """;

        using var connection = _connectionFactory.CreateConnection();

        var project = await connection.QuerySingleOrDefaultAsync<ProjectResponse>(
            sql, new { request.Id });

        return project is null
            ? Result.Failure<ProjectResponse>(
                Error.NotFound("Project", $"No se encontró el proyecto con ID '{request.Id}'."))
            : project;
    }
}
