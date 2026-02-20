using Edificia.Application.Auth.Commands.UpdateProfile;
using Edificia.Application.Auth.DTOs;
using FluentAssertions;

namespace Edificia.Application.Tests.Auth;

public class UpdateProfileMappingTests
{
    [Fact]
    public void ExplicitOperator_ShouldMapDtoFields()
    {
        // Arrange
        var request = new UpdateProfileRequest("New Name", "COL-123");

        // Act
        var command = (UpdateProfileCommand)request;

        // Assert
        command.UserId.Should().Be(Guid.Empty);
        command.FullName.Should().Be("New Name");
        command.CollegiateNumber.Should().Be("COL-123");
    }

    [Fact]
    public void ExplicitOperator_EnrichedWithJwtContext_ShouldSetUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateProfileRequest("Name Only", null);

        // Act
        var command = (UpdateProfileCommand)request with { UserId = userId };

        // Assert
        command.UserId.Should().Be(userId);
        command.CollegiateNumber.Should().BeNull();
    }
}
