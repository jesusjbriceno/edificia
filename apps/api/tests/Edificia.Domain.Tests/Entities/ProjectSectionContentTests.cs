using Edificia.Domain.Entities;
using Edificia.Domain.Enums;
using FluentAssertions;

namespace Edificia.Domain.Tests.Entities;

public class ProjectSectionContentTests
{
    private static Project CreateProjectWithTree(string contentTreeJson)
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);
        project.UpdateContentTree(contentTreeJson);
        return project;
    }

    private const string SampleTree = """
    {
        "chapters": [
            {
                "id": "md",
                "title": "Memoria Descriptiva",
                "content": "",
                "sections": [
                    {
                        "id": "md-1",
                        "title": "Agentes",
                        "content": "<p>Contenido original</p>",
                        "sections": [
                            {
                                "id": "md-1-1",
                                "title": "Promotor",
                                "content": "<p>Datos del promotor</p>"
                            }
                        ]
                    },
                    {
                        "id": "md-2",
                        "title": "Información previa",
                        "content": "<p>Info previa</p>"
                    }
                ]
            },
            {
                "id": "mc",
                "title": "Memoria Constructiva",
                "content": "",
                "sections": []
            }
        ]
    }
    """;

    [Fact]
    public void UpdateSectionContent_ShouldReturnTrue_WhenSectionExistsAtTopLevel()
    {
        var project = CreateProjectWithTree(SampleTree);

        var result = project.UpdateSectionContent("md", "<p>Nuevo contenido capítulo</p>");

        result.Should().BeTrue();
    }

    [Fact]
    public void UpdateSectionContent_ShouldUpdateContent_AtTopLevelChapter()
    {
        var project = CreateProjectWithTree(SampleTree);

        project.UpdateSectionContent("md", "<p>Nuevo contenido capítulo</p>");

        project.ContentTreeJson.Should().Contain("<p>Nuevo contenido cap");
    }

    [Fact]
    public void UpdateSectionContent_ShouldReturnTrue_WhenNestedSectionExists()
    {
        var project = CreateProjectWithTree(SampleTree);

        var result = project.UpdateSectionContent("md-1", "<p>Agentes actualizados</p>");

        result.Should().BeTrue();
    }

    [Fact]
    public void UpdateSectionContent_ShouldUpdateContent_InNestedSection()
    {
        var project = CreateProjectWithTree(SampleTree);

        project.UpdateSectionContent("md-1", "<p>Agentes actualizados</p>");

        project.ContentTreeJson.Should().Contain("<p>Agentes actualizados</p>");
        project.ContentTreeJson.Should().NotContain("<p>Contenido original</p>");
    }

    [Fact]
    public void UpdateSectionContent_ShouldReturnTrue_WhenDeeplyNestedSectionExists()
    {
        var project = CreateProjectWithTree(SampleTree);

        var result = project.UpdateSectionContent("md-1-1", "<p>Promotor actualizado</p>");

        result.Should().BeTrue();
    }

    [Fact]
    public void UpdateSectionContent_ShouldUpdateContent_InDeeplyNestedSection()
    {
        var project = CreateProjectWithTree(SampleTree);

        project.UpdateSectionContent("md-1-1", "<p>Promotor actualizado</p>");

        project.ContentTreeJson.Should().Contain("<p>Promotor actualizado</p>");
        project.ContentTreeJson.Should().NotContain("<p>Datos del promotor</p>");
    }

    [Fact]
    public void UpdateSectionContent_ShouldReturnFalse_WhenSectionDoesNotExist()
    {
        var project = CreateProjectWithTree(SampleTree);

        var result = project.UpdateSectionContent("nonexistent", "<p>Algo</p>");

        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateSectionContent_ShouldNotModifyTree_WhenSectionNotFound()
    {
        var project = CreateProjectWithTree(SampleTree);
        var originalJson = project.ContentTreeJson;

        project.UpdateSectionContent("nonexistent", "<p>Algo</p>");

        project.ContentTreeJson.Should().Be(originalJson);
    }

    [Fact]
    public void UpdateSectionContent_ShouldReturnFalse_WhenContentTreeIsNull()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);

        var result = project.UpdateSectionContent("md-1", "<p>Algo</p>");

        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateSectionContent_ShouldPreserveOtherSections()
    {
        var project = CreateProjectWithTree(SampleTree);

        project.UpdateSectionContent("md-1", "<p>Solo este cambia</p>");

        project.ContentTreeJson.Should().Contain("<p>Info previa</p>");
        project.ContentTreeJson.Should().Contain("<p>Datos del promotor</p>");
        project.ContentTreeJson.Should().Contain("Memoria Constructiva");
    }

    [Fact]
    public void UpdateSectionContent_ShouldAllowEmptyContent()
    {
        var project = CreateProjectWithTree(SampleTree);

        var result = project.UpdateSectionContent("md-1", "");

        result.Should().BeTrue();
    }

    [Fact]
    public void UpdateSectionContent_ShouldPreserveTreeStructure()
    {
        var project = CreateProjectWithTree(SampleTree);

        project.UpdateSectionContent("md-1", "<p>Actualizado</p>");

        // Should still contain the chapter titles and section ids
        project.ContentTreeJson.Should().Contain("\"md\"");
        project.ContentTreeJson.Should().Contain("\"md-1\"");
        project.ContentTreeJson.Should().Contain("\"md-1-1\"");
        project.ContentTreeJson.Should().Contain("\"md-2\"");
        project.ContentTreeJson.Should().Contain("\"mc\"");
        project.ContentTreeJson.Should().Contain("Memoria Descriptiva");
        project.ContentTreeJson.Should().Contain("Agentes");
    }

    [Fact]
    public void UpdateSectionContent_ShouldAddContentProperty_WhenNotPresent()
    {
        var treeWithoutContent = """
        {
            "chapters": [
                {
                    "id": "cap1",
                    "title": "Capítulo Sin Contenido",
                    "sections": []
                }
            ]
        }
        """;
        var project = CreateProjectWithTree(treeWithoutContent);

        var result = project.UpdateSectionContent("cap1", "<p>Contenido nuevo</p>");

        result.Should().BeTrue();
        project.ContentTreeJson.Should().Contain("<p>Contenido nuevo</p>");
    }

    [Fact]
    public void UpdateSectionContent_ShouldReturnFalse_WhenTreeHasNoChapters()
    {
        var emptyTree = """{"chapters": []}""";
        var project = CreateProjectWithTree(emptyTree);

        var result = project.UpdateSectionContent("md", "<p>Algo</p>");

        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateSectionContent_ShouldHandleSecondChapter()
    {
        var project = CreateProjectWithTree(SampleTree);

        var result = project.UpdateSectionContent("mc", "<p>Contenido constructiva</p>");

        result.Should().BeTrue();
        project.ContentTreeJson.Should().Contain("<p>Contenido constructiva</p>");
    }
}
