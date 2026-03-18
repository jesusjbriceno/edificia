namespace Edificia.Application.Templates.Queries;

public static class TemplateSqlQueries
{
    private const string TemplateColumns = """
            id                AS Id,
            name              AS Name,
            description       AS Description,
            template_type     AS TemplateType,
            version           AS Version,
            is_active         AS IsActive,
            original_file_name AS OriginalFileName,
            mime_type         AS MimeType,
            file_size_bytes   AS FileSizeBytes,
            created_at        AS CreatedAt,
            updated_at        AS UpdatedAt
        """;

    public static string GetAll(string whereClause) => $"""
        SELECT
        {TemplateColumns}
        FROM app_templates
        {whereClause}
        ORDER BY template_type ASC, version DESC
        """;
}
