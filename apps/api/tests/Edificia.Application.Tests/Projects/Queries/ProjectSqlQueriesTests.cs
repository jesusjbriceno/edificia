using Edificia.Application.Projects.Queries;
using FluentAssertions;

namespace Edificia.Application.Tests.Projects.Queries;

public class ProjectSqlQueriesTests
{
    [Fact]
    public void GetById_ShouldContainSelectFromProjects()
    {
        ProjectSqlQueries.GetById.Should().Contain("SELECT");
        ProjectSqlQueries.GetById.Should().Contain("FROM projects");
        ProjectSqlQueries.GetById.Should().Contain("WHERE id = @Id");
    }

    [Fact]
    public void GetById_ShouldContainAllProjectColumns()
    {
        var sql = ProjectSqlQueries.GetById;

        sql.Should().Contain("id              AS Id");
        sql.Should().Contain("title           AS Title");
        sql.Should().Contain("description     AS Description");
        sql.Should().Contain("address         AS Address");
        sql.Should().Contain("intervention_type AS InterventionType");
        sql.Should().Contain("is_loe_required AS IsLoeRequired");
        sql.Should().Contain("cadastral_reference AS CadastralReference");
        sql.Should().Contain("local_regulations AS LocalRegulations");
        sql.Should().Contain("status          AS Status");
        sql.Should().Contain("created_at      AS CreatedAt");
        sql.Should().Contain("updated_at      AS UpdatedAt");
    }

    [Fact]
    public void GetTree_ShouldContainTreeColumns()
    {
        var sql = ProjectSqlQueries.GetTree;

        sql.Should().Contain("SELECT");
        sql.Should().Contain("FROM projects");
        sql.Should().Contain("WHERE id = @ProjectId");
        sql.Should().Contain("AS ProjectId");
        sql.Should().Contain("AS InterventionType");
        sql.Should().Contain("AS IsLoeRequired");
        sql.Should().Contain("AS ContentTreeJson");
    }

    [Fact]
    public void CountBase_ShouldBeValidCountQuery()
    {
        ProjectSqlQueries.CountBase.Should().Be("SELECT COUNT(*) FROM projects");
    }

    [Fact]
    public void Count_WithEmptyWhere_ShouldReturnBaseCount()
    {
        var sql = ProjectSqlQueries.Count(string.Empty);

        sql.Should().Be("SELECT COUNT(*) FROM projects");
    }

    [Fact]
    public void Count_WithWhereClause_ShouldAppendClause()
    {
        var sql = ProjectSqlQueries.Count(" WHERE status = @Status");

        sql.Should().Be("SELECT COUNT(*) FROM projects WHERE status = @Status");
    }

    [Fact]
    public void GetPaged_ShouldContainOrderAndLimitOffset()
    {
        var sql = ProjectSqlQueries.GetPaged(string.Empty);

        sql.Should().Contain("ORDER BY created_at DESC");
        sql.Should().Contain("LIMIT @Limit OFFSET @Offset");
    }

    [Fact]
    public void GetPaged_WithWhereClause_ShouldIncludeFilter()
    {
        var sql = ProjectSqlQueries.GetPaged(" WHERE status = @Status");

        sql.Should().Contain("WHERE status = @Status");
        sql.Should().Contain("FROM projects");
        sql.Should().Contain("LIMIT @Limit OFFSET @Offset");
    }

    [Fact]
    public void GetPaged_ShouldContainAllProjectColumns()
    {
        var sql = ProjectSqlQueries.GetPaged(string.Empty);

        sql.Should().Contain("AS Title");
        sql.Should().Contain("AS Description");
        sql.Should().Contain("AS CreatedAt");
        sql.Should().Contain("AS UpdatedAt");
    }
}
