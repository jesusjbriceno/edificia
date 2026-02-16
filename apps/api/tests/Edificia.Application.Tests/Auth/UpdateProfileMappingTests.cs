using Edificia.Application.Auth.Commands.UpdateProfile;
using Edificia.Application.Auth.DTOs;
using FluentAssertions;

namespace Edificia.Application.Tests.Auth;

public class UpdateProfileMappingTests
{
    [Fact]
    public void UpdateProfileRequest_ShouldMapToCommand()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateProfileRequest("New Name", "COL-123");

        // Act
        var command = UpdateProfileCommand.Create(userId, request);

        // Assert
        command.UserId.Should().Be(userId);
        command.FullName.Should().Be("New Name");
        command.CollegiateNumber.Should().Be("COL-123");
    }

    [Fact]
    public void UpdateProfileRequest_ShouldMapNullCollegiateNumber()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateProfileRequest("Name Only", null);

        // Act
        var command = UpdateProfileCommand.Create(userId, request);

        // Assert
        command.CollegiateNumber.Should().BeNull();
    }
}
