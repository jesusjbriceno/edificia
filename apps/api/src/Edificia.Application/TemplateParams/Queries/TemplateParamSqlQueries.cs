namespace Edificia.Application.TemplateParams.Queries;

public static class TemplateParamSqlQueries
{
    public static string GetAll(string whereClause) => $"""
        SELECT
            id AS Id,
            key AS Key,
            display_name AS DisplayName,
            source_code AS SourceCode,
            formatter AS Formatter,
            is_active AS IsActive,
            created_at AS CreatedAt,
            updated_at AS UpdatedAt
        FROM template_params
        {whereClause}
        ORDER BY key ASC;
        """;
}
