using Edificia.Application.Users.Commands.CreateUser;
using Edificia.Application.Users.DTOs;
using FluentAssertions;

namespace Edificia.Application.Tests.Users;

public class CreateUserMappingTests
{
    [Fact]
    public void Create_ShouldMapAllFields()
    {
        // Arrange
        var createdByUserId = Guid.NewGuid();
        var request = new CreateUserRequest(
            "architect@edificia.dev",
            "María García",
            "Architect",
            "COL-789");

        // Act
        var command = CreateUserCommand.Create(createdByUserId, request);

        // Assert
        command.Email.Should().Be("architect@edificia.dev");
        command.FullName.Should().Be("María García");
        command.Role.Should().Be("Architect");
        command.CollegiateNumber.Should().Be("COL-789");
        command.CreatedByUserId.Should().Be(createdByUserId);
    }

    [Fact]
    public void Create_ShouldHandleNullCollegiateNumber()
    {
        // Arrange
        var createdByUserId = Guid.NewGuid();
        var request = new CreateUserRequest(
            "collaborator@edificia.dev",
            "Pedro López",
            "Collaborator");

        // Act
        var command = CreateUserCommand.Create(createdByUserId, request);

        // Assert
        command.CollegiateNumber.Should().BeNull();
    }
}
