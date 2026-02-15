using Edificia.Application.Ai.Services;
using Edificia.Domain.Enums;
using FluentAssertions;

namespace Edificia.Application.Tests.Ai.Services;

public class PromptTemplateServiceTests
{
    private readonly PromptTemplateService _service = new();

    // ── Intervention Type Injection ──

    [Fact]
    public void BuildSectionPrompt_ShouldInclude_ObraNueva_WhenNewConstruction()
    {
        var context = CreateContext(interventionType: InterventionType.NewConstruction);

        var result = _service.BuildSectionPrompt(context);

        result.Should().Contain("Obra Nueva");
    }

    [Fact]
    public void BuildSectionPrompt_ShouldInclude_Reforma_WhenReform()
    {
        var context = CreateContext(interventionType: InterventionType.Reform);

        var result = _service.BuildSectionPrompt(context);

        result.Should().Contain("Reforma");
    }

    [Fact]
    public void BuildSectionPrompt_ShouldInclude_Ampliacion_WhenExtension()
    {
        var context = CreateContext(interventionType: InterventionType.Extension);

        var result = _service.BuildSectionPrompt(context);

        result.Should().Contain("Ampliación");
    }

    // ── LOE Requirement Injection ──

    [Fact]
    public void BuildSectionPrompt_ShouldInclude_LoeApplicable_WhenRequired()
    {
        var context = CreateContext(isLoeRequired: true);

        var result = _service.BuildSectionPrompt(context);

        result.Should().Contain("LOE");
        result.Should().Contain("aplicable", because: "must indicate LOE applies");
    }

    [Fact]
    public void BuildSectionPrompt_ShouldInclude_LoeExempt_WhenNotRequired()
    {
        var context = CreateContext(isLoeRequired: false);

        var result = _service.BuildSectionPrompt(context);

        result.Should().Contain("LOE");
        result.Should().Contain("exento", because: "must indicate LOE exemption (Art 2.2)");
    }

    // ── Project Data Inclusion ──

    [Fact]
    public void BuildSectionPrompt_ShouldInclude_ProjectTitle()
    {
        var context = CreateContext(projectTitle: "Edificio Residencial Sol");

        var result = _service.BuildSectionPrompt(context);

        result.Should().Contain("Edificio Residencial Sol");
    }

    [Fact]
    public void BuildSectionPrompt_ShouldInclude_SectionId()
    {
        var context = CreateContext(sectionId: "DB-HE-01");

        var result = _service.BuildSectionPrompt(context);

        result.Should().Contain("DB-HE-01");
    }

    [Fact]
    public void BuildSectionPrompt_ShouldInclude_UserPrompt()
    {
        var context = CreateContext(userPrompt: "Describe los agentes intervinientes");

        var result = _service.BuildSectionPrompt(context);

        result.Should().Contain("Describe los agentes intervinientes");
    }

    [Fact]
    public void BuildSectionPrompt_ShouldInclude_Address_WhenProvided()
    {
        var context = CreateContext(address: "Calle Gran Vía 15, Madrid");

        var result = _service.BuildSectionPrompt(context);

        result.Should().Contain("Calle Gran Vía 15, Madrid");
    }

    [Fact]
    public void BuildSectionPrompt_ShouldNotInclude_AddressSection_WhenNull()
    {
        var context = CreateContext(address: null);

        var result = _service.BuildSectionPrompt(context);

        result.Should().NotContain("Dirección");
    }

    [Fact]
    public void BuildSectionPrompt_ShouldInclude_LocalRegulations_WhenProvided()
    {
        var context = CreateContext(localRegulations: "PGOU Madrid 2024");

        var result = _service.BuildSectionPrompt(context);

        result.Should().Contain("PGOU Madrid 2024");
    }

    [Fact]
    public void BuildSectionPrompt_ShouldNotInclude_RegulationsSection_WhenNull()
    {
        var context = CreateContext(localRegulations: null);

        var result = _service.BuildSectionPrompt(context);

        result.Should().NotContain("Normativa local");
    }

    // ── Existing Content (Context) ──

    [Fact]
    public void BuildSectionPrompt_ShouldInclude_ExistingContent_WhenProvided()
    {
        var context = CreateContext(existingContent: "<p>Contenido actual de la sección</p>");

        var result = _service.BuildSectionPrompt(context);

        result.Should().Contain("<p>Contenido actual de la sección</p>");
    }

    [Fact]
    public void BuildSectionPrompt_ShouldNotInclude_ExistingContentSection_WhenNull()
    {
        var context = CreateContext(existingContent: null);

        var result = _service.BuildSectionPrompt(context);

        result.Should().NotContain("Contenido existente");
    }

    // ── Structure Validation ──

    [Fact]
    public void BuildSectionPrompt_ShouldHave_ProjectContextBeforeInstruction()
    {
        var context = CreateContext(
            projectTitle: "Proyecto Test",
            userPrompt: "Describe agentes");

        var result = _service.BuildSectionPrompt(context);

        var projectIdx = result.IndexOf("Proyecto Test");
        var promptIdx = result.IndexOf("Describe agentes");

        projectIdx.Should().BeLessThan(promptIdx,
            because: "project context must appear before user instruction");
    }

    [Fact]
    public void BuildSectionPrompt_ShouldContain_AllSections_ForCompleteContext()
    {
        var context = new SectionPromptContext(
            SectionId: "DB-SE-01",
            UserPrompt: "Redacta la sección de seguridad estructural",
            ExistingContent: "<p>Borrador anterior</p>",
            ProjectTitle: "Torre Norte",
            InterventionType: InterventionType.NewConstruction,
            IsLoeRequired: true,
            Address: "Av. Diagonal 123, Barcelona",
            LocalRegulations: "Ordenanza Metropolitana de Barcelona");

        var result = _service.BuildSectionPrompt(context);

        result.Should().Contain("Torre Norte");
        result.Should().Contain("Obra Nueva");
        result.Should().Contain("LOE");
        result.Should().Contain("DB-SE-01");
        result.Should().Contain("Av. Diagonal 123, Barcelona");
        result.Should().Contain("Ordenanza Metropolitana de Barcelona");
        result.Should().Contain("Redacta la sección de seguridad estructural");
        result.Should().Contain("<p>Borrador anterior</p>");
    }

    [Fact]
    public void BuildSectionPrompt_ShouldWork_WithMinimalContext()
    {
        var context = new SectionPromptContext(
            SectionId: "md-1",
            UserPrompt: "Genera texto",
            ExistingContent: null,
            ProjectTitle: "Proyecto Mínimo",
            InterventionType: InterventionType.Reform,
            IsLoeRequired: false,
            Address: null,
            LocalRegulations: null);

        var result = _service.BuildSectionPrompt(context);

        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("Proyecto Mínimo");
        result.Should().Contain("Reforma");
        result.Should().Contain("Genera texto");
    }

    [Fact]
    public void BuildSectionPrompt_Result_ShouldNotBeEmpty()
    {
        var context = CreateContext();

        var result = _service.BuildSectionPrompt(context);

        result.Should().NotBeNullOrWhiteSpace();
    }

    // ── Reform-specific guidance ──

    [Fact]
    public void BuildSectionPrompt_ForReform_ShouldInclude_ReformGuidance()
    {
        var context = CreateContext(
            interventionType: InterventionType.Reform,
            isLoeRequired: false);

        var result = _service.BuildSectionPrompt(context);

        result.Should().Contain("Reforma",
            because: "reform projects need specific CTE adaptation");
    }

    // ── Helper ──

    private static SectionPromptContext CreateContext(
        string sectionId = "md-1",
        string userPrompt = "Describe los agentes",
        string? existingContent = null,
        string projectTitle = "Proyecto Test",
        InterventionType interventionType = InterventionType.NewConstruction,
        bool isLoeRequired = true,
        string? address = null,
        string? localRegulations = null)
    {
        return new SectionPromptContext(
            SectionId: sectionId,
            UserPrompt: userPrompt,
            ExistingContent: existingContent,
            ProjectTitle: projectTitle,
            InterventionType: interventionType,
            IsLoeRequired: isLoeRequired,
            Address: address,
            LocalRegulations: localRegulations);
    }
}
