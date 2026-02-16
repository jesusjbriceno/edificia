namespace Edificia.Application.Projects.Queries;

/// <summary>
/// Centralized SQL constants for Dapper-based project queries.
/// All raw SQL used by Project query handlers is defined here
/// to facilitate review, reuse, and maintenance.
/// </summary>
public static class ProjectSqlQueries
{
    /// <summary>
    /// Column list for the full project read model.
    /// Shared between GetById and GetPaged queries.
    /// </summary>
    private const string ProjectColumns = """
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
        """;

    /// <summary>
    /// Retrieves a single project by its ID.
    /// Parameter: @Id (Guid).
    /// </summary>
    public const string GetById = $"""
        SELECT
        {ProjectColumns}
        FROM projects
        WHERE id = @Id
        """;

    /// <summary>
    /// Retrieves the content tree and intervention metadata for a project.
    /// Parameter: @ProjectId (Guid).
    /// </summary>
    public const string GetTree = """
        SELECT
            id                  AS ProjectId,
            intervention_type   AS InterventionType,
            is_loe_required     AS IsLoeRequired,
            content_tree_json   AS ContentTreeJson
        FROM projects
        WHERE id = @ProjectId
        """;

    /// <summary>
    /// Count query template. Append WHERE clause dynamically.
    /// </summary>
    public const string CountBase = "SELECT COUNT(*) FROM projects";

    /// <summary>
    /// Builds the paginated data query with the given WHERE clause.
    /// Parameters: @Limit (int), @Offset (int), plus any filter parameters.
    /// </summary>
    public static string GetPaged(string whereClause) => $"""
        SELECT
        {ProjectColumns}
        FROM projects
        {whereClause}
        ORDER BY created_at DESC
        LIMIT @Limit OFFSET @Offset
        """;

    /// <summary>
    /// Builds the count query with the given WHERE clause.
    /// </summary>
    public static string Count(string whereClause) =>
        $"{CountBase}{whereClause}";
}
