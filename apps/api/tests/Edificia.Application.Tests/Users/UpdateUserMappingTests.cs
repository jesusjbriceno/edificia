using Edificia.Application.Users.Commands.UpdateUser;
using Edificia.Application.Users.DTOs;
using FluentAssertions;

namespace Edificia.Application.Tests.Users;

public class UpdateUserMappingTests
{
    [Fact]
    public void ExplicitOperator_ShouldMapDtoFields()
    {
        // Arrange
        var request = new UpdateUserRequest(
            "María García Actualizada",
            "Architect",
            "COL-999");

        // Act
        var command = (UpdateUserCommand)request;

        // Assert
        command.FullName.Should().Be("María García Actualizada");
        command.Role.Should().Be("Architect");
        command.CollegiateNumber.Should().Be("COL-999");
        command.UserId.Should().Be(Guid.Empty);
        command.UpdatedByUserId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void ExplicitOperator_EnrichedWithRouteAndJwtContext_ShouldSetAllIds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updatedByUserId = Guid.NewGuid();
        var request = new UpdateUserRequest(
            "María García Actualizada",
            "Architect",
            "COL-999");

        // Act
        var command = (UpdateUserCommand)request with { UserId = userId, UpdatedByUserId = updatedByUserId };

        // Assert
        command.UserId.Should().Be(userId);
        command.UpdatedByUserId.Should().Be(updatedByUserId);
    }
}
