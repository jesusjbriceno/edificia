namespace Edificia.Application.Users.Queries;

/// <summary>
/// Centralized SQL constants for Dapper-based user queries.
/// Queries join asp_net_users with asp_net_user_roles and asp_net_roles
/// to include the role name in the response.
/// </summary>
public static class UserSqlQueries
{
    /// <summary>
    /// Column list for the user read model.
    /// Shared between GetById and GetPaged queries.
    /// </summary>
    private const string UserColumns = """
                u.id                     AS Id,
                u.email                  AS Email,
                u.full_name              AS FullName,
                u.collegiate_number      AS CollegiateNumber,
                u.is_active              AS IsActive,
                u.must_change_password   AS MustChangePassword,
                COALESCE(r.name, '')     AS Role,
                u.created_at             AS CreatedAt,
                u.updated_at             AS UpdatedAt
        """;

    /// <summary>
    /// JOIN clause for asp_net_user_roles and asp_net_roles.
    /// </summary>
    private const string RoleJoin = """
            LEFT JOIN asp_net_user_roles ur ON u.id = ur.user_id
            LEFT JOIN asp_net_roles r ON ur.role_id = r.id
        """;

    /// <summary>
    /// Retrieves a single user by ID with their role.
    /// Parameter: @Id (Guid).
    /// </summary>
    public const string GetById = $"""
        SELECT
        {UserColumns}
        FROM asp_net_users u
        {RoleJoin}
        WHERE u.id = @Id
        """;

    /// <summary>
    /// Count query template. Append WHERE clause dynamically.
    /// </summary>
    public const string CountBase = $"""
        SELECT COUNT(DISTINCT u.id) 
        FROM asp_net_users u
        {RoleJoin}
        """;

    /// <summary>
    /// Builds the paginated data query with the given WHERE clause.
    /// Parameters: @Limit (int), @Offset (int), plus any filter parameters.
    /// </summary>
    public static string GetPaged(string whereClause) => $"""
        SELECT
        {UserColumns}
        FROM asp_net_users u
        {RoleJoin}
        {whereClause}
        ORDER BY u.created_at DESC
        LIMIT @Limit OFFSET @Offset
        """;

    /// <summary>
    /// Builds the count query with the given WHERE clause.
    /// </summary>
    public static string Count(string whereClause) =>
        $"{CountBase}{whereClause}";
}
