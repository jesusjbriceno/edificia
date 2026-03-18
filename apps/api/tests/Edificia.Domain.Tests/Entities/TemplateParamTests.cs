using Edificia.Domain.Entities;
using Edificia.Domain.Exceptions;
using FluentAssertions;

namespace Edificia.Domain.Tests.Entities;

public class TemplateParamTests
{
    [Fact]
    public void Create_ShouldNormalizeFieldsAndSetDefaults()
    {
        var templateParam = TemplateParam.Create(
            key: " project_title ",
            displayName: "Titulo del proyecto",
            sourceCode: " project_title ",
            formatter: " lower ");

        templateParam.Key.Should().Be("PROJECT_TITLE");
        templateParam.SourceCode.Should().Be("PROJECT_TITLE");
        templateParam.Formatter.Should().Be("LOWER");
        templateParam.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithInvalidKey_ShouldThrowBusinessRuleException()
    {
        var act = () => TemplateParam.Create(
            key: "PROJECT-TITLE",
            displayName: "Titulo",
            sourceCode: "PROJECT_TITLE");

        act.Should().Throw<BusinessRuleException>()
            .Where(ex => ex.Code == "TemplateParam.InvalidKeyFormat");
    }

    [Fact]
    public void Create_WithUnsupportedSourceCode_ShouldThrowBusinessRuleException()
    {
        var act = () => TemplateParam.Create(
            key: "PROJECT_TITLE",
            displayName: "Titulo",
            sourceCode: "PROJECT_CLIENT");

        act.Should().Throw<BusinessRuleException>()
            .Where(ex => ex.Code == "TemplateParam.UnsupportedSourceCode");
    }

    [Fact]
    public void Create_WithUnsupportedFormatter_ShouldThrowBusinessRuleException()
    {
        var act = () => TemplateParam.Create(
            key: "PROJECT_TITLE",
            displayName: "Titulo",
            sourceCode: "PROJECT_TITLE",
            formatter: "DATE");

        act.Should().Throw<BusinessRuleException>()
            .Where(ex => ex.Code == "TemplateParam.UnsupportedFormatter");
    }

    [Fact]
    public void UpdateMetadata_WithUnsupportedSourceCode_ShouldThrowBusinessRuleException()
    {
        var templateParam = TemplateParam.Create(
            key: "PROJECT_TITLE",
            displayName: "Titulo",
            sourceCode: "PROJECT_TITLE");

        var act = () => templateParam.UpdateMetadata(
            displayName: "Titulo actualizado",
            sourceCode: "PROJECT_CLIENT",
            formatter: null);

        act.Should().Throw<BusinessRuleException>()
            .Where(ex => ex.Code == "TemplateParam.UnsupportedSourceCode");
    }
}
