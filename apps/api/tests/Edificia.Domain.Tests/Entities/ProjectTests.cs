using Edificia.Domain.Entities;
using Edificia.Domain.Enums;
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
            createdByUserId: Guid.NewGuid(),
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
            isLoeRequired: false,
            createdByUserId: Guid.NewGuid());

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
        var project1 = Project.Create("P1", InterventionType.NewConstruction, true, Guid.NewGuid());
        var project2 = Project.Create("P2", InterventionType.Reform, false, Guid.NewGuid());

        project1.Id.Should().NotBe(project2.Id);
    }

    [Fact]
    public void UpdateSettings_ShouldModifyAllProperties()
    {
        var project = Project.Create("Original", InterventionType.NewConstruction, true, Guid.NewGuid());

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
            Guid.NewGuid(),
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
        var project = Project.Create("Test", InterventionType.NewConstruction, true, Guid.NewGuid());

        project.StartRedaction();

        project.Status.Should().Be(ProjectStatus.InProgress);
    }

    [Fact]
    public void Complete_ShouldTransitionToCompleted()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true, Guid.NewGuid());
        project.StartRedaction();

        project.Complete();

        project.Status.Should().Be(ProjectStatus.Completed);
    }

    [Fact]
    public void Archive_ShouldTransitionToArchived()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true, Guid.NewGuid());

        project.Archive();

        project.Status.Should().Be(ProjectStatus.Archived);
    }

    [Fact]
    public void UpdateContentTree_ShouldSetContentTreeJson()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true, Guid.NewGuid());
        var json = """{"chapters":[{"id":"cap1","title":"Memoria Descriptiva"}]}""";

        project.UpdateContentTree(json);

        project.ContentTreeJson.Should().Be(json);
    }

    [Fact]
    public void UpdateContentTree_ShouldAllowOverwrite()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true, Guid.NewGuid());
        project.UpdateContentTree("""{"v":1}""");

        project.UpdateContentTree("""{"v":2}""");

        project.ContentTreeJson.Should().Be("""{"v":2}""");
    }

    [Fact]
    public void Create_ShouldSetCreatedByUserId()
    {
        var userId = Guid.NewGuid();
        var project = Project.Create("Test", InterventionType.NewConstruction, true, userId);

        project.CreatedByUserId.Should().Be(userId);
    }

    [Fact]
    public void AddMember_ShouldAddMemberToProject()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true, Guid.NewGuid());
        var userId = Guid.NewGuid();

        project.AddMember(userId, ProjectMemberRole.Editor);

        project.Members.Should().HaveCount(1);
        project.Members.First().UserId.Should().Be(userId);
        project.Members.First().Role.Should().Be(ProjectMemberRole.Editor);
    }

    [Fact]
    public void AddMember_ShouldNotDuplicate_WhenUserAlreadyAdded()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true, Guid.NewGuid());
        var userId = Guid.NewGuid();

        project.AddMember(userId, ProjectMemberRole.Editor);
        project.AddMember(userId, ProjectMemberRole.Viewer);

        project.Members.Should().HaveCount(1);
        project.Members.First().Role.Should().Be(ProjectMemberRole.Editor);
    }

    [Fact]
    public void RemoveMember_ShouldRemoveMemberFromProject()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true, Guid.NewGuid());
        var userId = Guid.NewGuid();
        project.AddMember(userId, ProjectMemberRole.Editor);

        project.RemoveMember(userId);

        project.Members.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMember_ShouldDoNothing_WhenUserNotMember()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true, Guid.NewGuid());

        project.RemoveMember(Guid.NewGuid());

        project.Members.Should().BeEmpty();
    }

    [Fact]
    public void AddMember_ShouldAllowMultipleMembers()
    {
        var project = Project.Create("Test", InterventionType.NewConstruction, true, Guid.NewGuid());
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var user3 = Guid.NewGuid();

        project.AddMember(user1, ProjectMemberRole.Owner);
        project.AddMember(user2, ProjectMemberRole.Editor);
        project.AddMember(user3, ProjectMemberRole.Viewer);

        project.Members.Should().HaveCount(3);
    }
}