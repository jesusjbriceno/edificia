using Edificia.Domain.Entities;
using Edificia.Domain.Enums;
using Edificia.Domain.Exceptions;
using FluentAssertions;

namespace Edificia.Domain.Tests.Entities;

public class ProjectTests
{
    [Fact]
    public void Create_ShouldInitializeProjectWithCorrectValues()
    {
        var project = Project.Create(
            title: "Vivienda Unifamiliar",
            interventionType: InterventionType.NewConstruction,
            isLoeRequired: true,
            description: "Proyecto de obra nueva",
            address: "Calle Mayor 1, Madrid",
            cadastralReference: "1234567AB1234N",
            localRegulations: "PGOU Madrid 2023");

        project.Id.Should().NotBeEmpty();
        project.Title.Should().Be("Vivienda Unifamiliar");
        project.InterventionType.Should().Be(InterventionType.NewConstruction);
        project.IsLoeRequired.Should().BeTrue();
        project.Description.Should().Be("Proyecto de obra nueva");
        project.Address.Should().Be("Calle Mayor 1, Madrid");
        project.CadastralReference.Should().Be("1234567AB1234N");
        project.LocalRegulations.Should().Be("PGOU Madrid 2023");
        project.Status.Should().Be(ProjectStatus.Draft);
        project.ContentTreeJson.Should().BeNull();
    }

    [Fact]
    public void Create_WithMinimalParams_ShouldSetOptionalToNull()
    {
        var project = Project.Create(
            title: "Reforma local",
            interventionType: InterventionType.Reform,
            isLoeRequired: false);

        project.Title.Should().Be("Reforma local");
        project.InterventionType.Should().Be(InterventionType.Reform);
        project.IsLoeRequired.Should().BeFalse();
        project.Description.Should().BeNull();
        project.Address.Should().BeNull();
        project.CadastralReference.Should().BeNull();
        project.LocalRegulations.Should().BeNull();
        project.Status.Should().Be(ProjectStatus.Draft);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var project1 = Project.Create("P1", InterventionType.NewConstruction, true);
        var project2 = Project.Create("P2", InterventionType.Reform, false);

        project1.Id.Should().NotBe(project2.Id);
    }

    [Fact]
    public void UpdateSettings_ShouldModifyAllProperties()
    {
        var project = Project.Create("Original", InterventionType.NewConstruction, true);

        project.UpdateSettings(
            title: "Actualizado",
            interventionType: InterventionType.Reform,
            isLoeRequired: false,
            description: "Nueva descripción",
            address: "Av. Diagonal 100, Barcelona",
            cadastralReference: "9876543CD",
            localRegulations: "POUM Barcelona");

        project.Title.Should().Be("Actualizado");
        project.InterventionType.Should().Be(InterventionType.Reform);
        project.IsLoeRequired.Should().BeFalse();
        project.Description.Should().Be("Nueva descripción");
        project.Address.Should().Be("Av. Diagonal 100, Barcelona");
        project.CadastralReference.Should().Be("9876543CD");
        project.LocalRegulations.Should().Be("POUM Barcelona");
    }

    [Fact]
    public void UpdateSettings_ShouldClearOptionalFields_WhenNull()
    {
        var project = Project.Create(
            "Con datos",
            InterventionType.NewConstruction,
            true,
            description: "Algo",
            address: "Calle X");

        project.UpdateSettings("Sin datos", InterventionType.Extension, false);

        project.Description.Should().BeNull();
        project.Address.Should().BeNull();
        project.CadastralReference.Should().BeNull();
        project.LocalRegulations.Should().BeNull();
    }

    [Fact]
    public void StartRedaction_ShouldTransitionToInProgress()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);

        project.StartRedaction();

        project.Status.Should().Be(ProjectStatus.InProgress);
    }

    [Fact]
    public void Complete_FromPendingReview_ShouldTransitionToCompleted()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);
        project.SubmitForReview();

        project.Complete();

        project.Status.Should().Be(ProjectStatus.Completed);
    }

    [Fact]
    public void Archive_FromCompleted_ShouldTransitionToArchived_Legacy()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);
        project.SubmitForReview();
        project.Approve();

        project.Archive();

        project.Status.Should().Be(ProjectStatus.Archived);
    }

    [Fact]
    public void UpdateContentTree_ShouldSetContentTreeJson()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);
        var json = """{"chapters":[{"id":"cap1","title":"Memoria Descriptiva"}]}""";

        project.UpdateContentTree(json);

        project.ContentTreeJson.Should().Be(json);
    }

    [Fact]
    public void UpdateContentTree_ShouldAllowOverwrite()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);
        project.UpdateContentTree("""{"v":1}""");

        project.UpdateContentTree("""{"v":2}""");

        project.ContentTreeJson.Should().Be("""{"v":2}""");
    }

    // ─── Review Workflow: SubmitForReview ───────────────────────────

    [Fact]
    public void SubmitForReview_FromDraft_ShouldTransitionToPendingReview()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);

        project.SubmitForReview();

        project.Status.Should().Be(ProjectStatus.PendingReview);
    }

    [Fact]
    public void SubmitForReview_FromInProgress_ShouldTransitionToPendingReview()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);
        project.StartRedaction();

        project.SubmitForReview();

        project.Status.Should().Be(ProjectStatus.PendingReview);
    }

    [Fact]
    public void SubmitForReview_FromCompleted_ShouldThrowBusinessRuleException()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);
        project.SubmitForReview();
        project.Approve();

        var act = () => project.SubmitForReview();

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*no se puede enviar a revisión*");
    }

    [Fact]
    public void SubmitForReview_FromArchived_ShouldThrowBusinessRuleException()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);
        project.SubmitForReview();
        project.Approve();
        project.Archive();

        var act = () => project.SubmitForReview();

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*no se puede enviar a revisión*");
    }

    [Fact]
    public void SubmitForReview_FromPendingReview_ShouldThrowBusinessRuleException()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);
        project.SubmitForReview();

        var act = () => project.SubmitForReview();

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*ya está pendiente de revisión*");
    }

    // ─── Review Workflow: Approve ──────────────────────────────────

    [Fact]
    public void Approve_FromPendingReview_ShouldTransitionToCompleted()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);
        project.SubmitForReview();

        project.Approve();

        project.Status.Should().Be(ProjectStatus.Completed);
    }

    [Fact]
    public void Approve_FromDraft_ShouldThrowBusinessRuleException()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);

        var act = () => project.Approve();

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*solo se puede aprobar*pendiente de revisión*");
    }

    [Fact]
    public void Approve_FromInProgress_ShouldThrowBusinessRuleException()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);
        project.StartRedaction();

        var act = () => project.Approve();

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*solo se puede aprobar*pendiente de revisión*");
    }

    // ─── Review Workflow: Reject ───────────────────────────────────

    [Fact]
    public void Reject_FromPendingReview_ShouldTransitionToDraft()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);
        project.SubmitForReview();

        project.Reject();

        project.Status.Should().Be(ProjectStatus.Draft);
    }

    [Fact]
    public void Reject_FromDraft_ShouldThrowBusinessRuleException()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);

        var act = () => project.Reject();

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*solo se puede rechazar*pendiente de revisión*");
    }

    [Fact]
    public void Reject_FromCompleted_ShouldThrowBusinessRuleException()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);
        project.SubmitForReview();
        project.Approve();

        var act = () => project.Reject();

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*solo se puede rechazar*pendiente de revisión*");
    }

    // ─── Review Workflow: Archive (guard) ──────────────────────────

    [Fact]
    public void Archive_FromDraft_ShouldThrowBusinessRuleException()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);

        var act = () => project.Archive();

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*solo se puede archivar*completado*");
    }

    [Fact]
    public void Archive_FromArchived_ShouldThrowBusinessRuleException()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);
        project.SubmitForReview();
        project.Approve();
        project.Archive();

        var act = () => project.Archive();

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*ya está archivado*");
    }

    // ─── Review Workflow: Readonly guard ───────────────────────────

    [Fact]
    public void UpdateSectionContent_WhenPendingReview_ShouldThrowBusinessRuleException()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);
        project.UpdateContentTree("""{"chapters":[{"id":"s1","title":"Test","content":"old"}]}""");
        project.SubmitForReview();

        var act = () => project.UpdateSectionContent("s1", "<p>new</p>");

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*no se puede editar*pendiente de revisión*");
    }

    [Fact]
    public void UpdateSectionContent_WhenCompleted_ShouldThrowBusinessRuleException()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);
        project.UpdateContentTree("""{"chapters":[{"id":"s1","title":"Test","content":"old"}]}""");
        project.SubmitForReview();
        project.Approve();

        var act = () => project.UpdateSectionContent("s1", "<p>new</p>");

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*no se puede editar*completad*");
    }

    [Fact]
    public void UpdateContentTree_WhenPendingReview_ShouldThrowBusinessRuleException()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);
        project.SubmitForReview();

        var act = () => project.UpdateContentTree("""{"chapters":[]}""");

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*no se puede editar*");
    }

    // ─── Review Workflow: Full cycle ───────────────────────────────

    [Fact]
    public void FullReviewCycle_RejectThenResubmit_ShouldWork()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true);

        // First submission → rejected
        project.SubmitForReview();
        project.Status.Should().Be(ProjectStatus.PendingReview);

        project.Reject();
        project.Status.Should().Be(ProjectStatus.Draft);

        // Re-submit → approved
        project.SubmitForReview();
        project.Status.Should().Be(ProjectStatus.PendingReview);

        project.Approve();
        project.Status.Should().Be(ProjectStatus.Completed);

        // Archive
        project.Archive();
        project.Status.Should().Be(ProjectStatus.Archived);
    }
}
