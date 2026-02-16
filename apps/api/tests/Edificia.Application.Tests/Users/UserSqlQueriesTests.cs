using Edificia.Application.Users.Queries;
using FluentAssertions;

namespace Edificia.Application.Tests.Users;

public class UserSqlQueriesTests
{
    [Fact]
    public void GetById_ShouldContainSelectFromUsers()
    {
        UserSqlQueries.GetById.Should().Contain("SELECT");
        UserSqlQueries.GetById.Should().Contain("FROM asp_net_users u");
        UserSqlQueries.GetById.Should().Contain("WHERE u.id = @Id");
    }

    [Fact]
    public void GetById_ShouldContainAllUserColumns()
    {
        var sql = UserSqlQueries.GetById;

        sql.Should().Contain("AS Id");
        sql.Should().Contain("AS Email");
        sql.Should().Contain("AS FullName");
        sql.Should().Contain("AS CollegiateNumber");
        sql.Should().Contain("AS IsActive");
        sql.Should().Contain("AS MustChangePassword");
        sql.Should().Contain("AS Role");
        sql.Should().Contain("AS CreatedAt");
        sql.Should().Contain("AS UpdatedAt");
    }

    [Fact]
    public void GetById_ShouldContainRoleJoin()
    {
        var sql = UserSqlQueries.GetById;

        sql.Should().Contain("LEFT JOIN asp_net_user_roles ur ON u.id = ur.user_id");
        sql.Should().Contain("LEFT JOIN asp_net_roles r ON ur.role_id = r.id");
    }

    [Fact]
    public void CountBase_ShouldBeValidCountQuery()
    {
        UserSqlQueries.CountBase.Should().Contain("SELECT COUNT(DISTINCT u.id)");
        UserSqlQueries.CountBase.Should().Contain("FROM asp_net_users u");
    }

    [Fact]
    public void Count_WithEmptyWhere_ShouldReturnBaseCount()
    {
        var sql = UserSqlQueries.Count(string.Empty);

        sql.Should().Contain("SELECT COUNT(DISTINCT u.id)");
        sql.Should().Contain("FROM asp_net_users u");
    }

    [Fact]
    public void Count_WithWhereClause_ShouldAppendClause()
    {
        var sql = UserSqlQueries.Count(" WHERE u.is_active = @IsActive");

        sql.Should().Contain("WHERE u.is_active = @IsActive");
        sql.Should().Contain("FROM asp_net_users u");
    }

    [Fact]
    public void GetPaged_ShouldContainOrderAndLimitOffset()
    {
        var sql = UserSqlQueries.GetPaged(string.Empty);

        sql.Should().Contain("ORDER BY u.created_at DESC");
        sql.Should().Contain("LIMIT @Limit OFFSET @Offset");
    }

    [Fact]
    public void GetPaged_WithWhereClause_ShouldIncludeFilter()
    {
        var sql = UserSqlQueries.GetPaged(" WHERE r.name = @Role");

        sql.Should().Contain("WHERE r.name = @Role");
        sql.Should().Contain("FROM asp_net_users u");
        sql.Should().Contain("LIMIT @Limit OFFSET @Offset");
    }

    [Fact]
    public void GetPaged_ShouldContainAllUserColumns()
    {
        var sql = UserSqlQueries.GetPaged(string.Empty);

        sql.Should().Contain("AS Id");
        sql.Should().Contain("AS Email");
        sql.Should().Contain("AS FullName");
        sql.Should().Contain("AS Role");
        sql.Should().Contain("AS CreatedAt");
    }

    [Fact]
    public void GetPaged_ShouldContainRoleJoin()
    {
        var sql = UserSqlQueries.GetPaged(string.Empty);

        sql.Should().Contain("LEFT JOIN asp_net_user_roles ur ON u.id = ur.user_id");
        sql.Should().Contain("LEFT JOIN asp_net_roles r ON ur.role_id = r.id");
    }
}
