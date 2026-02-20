using Edificia.Domain.Entities;
using Edificia.Domain.Enums;
using FluentAssertions;

namespace Edificia.Domain.Tests.Entities;

public class ProjectMemberTests
{
    [Fact]
    public void Create_ShouldInitializeWithCorrectValues()
    {
        var projectId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var member = ProjectMember.Create(projectId, userId, ProjectMemberRole.Owner);

        member.Id.Should().NotBeEmpty();
        member.ProjectId.Should().Be(projectId);
        member.UserId.Should().Be(userId);
        member.Role.Should().Be(ProjectMemberRole.Owner);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var member1 = ProjectMember.Create(Guid.NewGuid(), Guid.NewGuid(), ProjectMemberRole.Editor);
        var member2 = ProjectMember.Create(Guid.NewGuid(), Guid.NewGuid(), ProjectMemberRole.Viewer);

        member1.Id.Should().NotBe(member2.Id);
    }

    [Fact]
    public void ChangeRole_ShouldUpdateRole()
    {
        var member = ProjectMember.Create(Guid.NewGuid(), Guid.NewGuid(), ProjectMemberRole.Viewer);

        member.ChangeRole(ProjectMemberRole.Editor);

        member.Role.Should().Be(ProjectMemberRole.Editor);
    }

    [Fact]
    public void ChangeRole_ShouldAllowChangingToOwner()
    {
        var member = ProjectMember.Create(Guid.NewGuid(), Guid.NewGuid(), ProjectMemberRole.Editor);

        member.ChangeRole(ProjectMemberRole.Owner);

        member.Role.Should().Be(ProjectMemberRole.Owner);
    }

    [Theory]
    [InlineData(ProjectMemberRole.Owner)]
    [InlineData(ProjectMemberRole.Editor)]
    [InlineData(ProjectMemberRole.Viewer)]
    public void Create_ShouldAcceptAllRoles(ProjectMemberRole role)
    {
        var member = ProjectMember.Create(Guid.NewGuid(), Guid.NewGuid(), role);

        member.Role.Should().Be(role);
    }
}
