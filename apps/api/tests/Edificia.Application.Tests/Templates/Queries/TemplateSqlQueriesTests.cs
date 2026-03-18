using Edificia.Application.Templates.Queries;
using FluentAssertions;

namespace Edificia.Application.Tests.Templates.Queries;

public class TemplateSqlQueriesTests
{
    [Fact]
    public void GetAll_ShouldContainSelectFromAppTemplates()
    {
        var sql = TemplateSqlQueries.GetAll(string.Empty);

        sql.Should().Contain("SELECT");
        sql.Should().Contain("FROM app_templates");
        sql.Should().Contain("ORDER BY template_type ASC, version DESC");
    }

    [Fact]
    public void GetAll_ShouldAppendWhereClause()
    {
        var sql = TemplateSqlQueries.GetAll(" WHERE template_type = @TemplateType");

        sql.Should().Contain("WHERE template_type = @TemplateType");
    }
}
