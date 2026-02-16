using Edificia.Application.Users.Commands.UpdateUser;
using Edificia.Application.Users.DTOs;
using FluentAssertions;

namespace Edificia.Application.Tests.Users;

public class UpdateUserMappingTests
{
    [Fact]
    public void Create_ShouldMapAllFields()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updatedByUserId = Guid.NewGuid();
        var request = new UpdateUserRequest(
            "María García Actualizada",
            "Architect",
            "COL-999");

        // Act
        var command = UpdateUserCommand.Create(userId, updatedByUserId, request);

        // Assert
        command.UserId.Should().Be(userId);
        command.FullName.Should().Be("María García Actualizada");
        command.Role.Should().Be("Architect");
        command.CollegiateNumber.Should().Be("COL-999");
        command.UpdatedByUserId.Should().Be(updatedByUserId);
    }
}
