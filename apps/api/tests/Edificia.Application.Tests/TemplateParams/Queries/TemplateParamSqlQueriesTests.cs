using Edificia.Application.TemplateParams.Queries;
using FluentAssertions;

namespace Edificia.Application.Tests.TemplateParams.Queries;

public class TemplateParamSqlQueriesTests
{
    [Fact]
    public void GetAll_ShouldContainSelectFromTemplateParams()
    {
        var sql = TemplateParamSqlQueries.GetAll(string.Empty);

        sql.Should().Contain("SELECT");
        sql.Should().Contain("FROM template_params");
        sql.Should().Contain("ORDER BY key ASC");
    }

    [Fact]
    public void GetAll_ShouldAppendWhereClause()
    {
        var sql = TemplateParamSqlQueries.GetAll("WHERE is_active = @IsActive");

        sql.Should().Contain("WHERE is_active = @IsActive");
    }
}
