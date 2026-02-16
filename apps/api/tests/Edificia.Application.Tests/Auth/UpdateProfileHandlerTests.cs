using Edificia.Application.Auth.Commands.Login;
using Edificia.Application.Auth.Commands.UpdateProfile;
using Edificia.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Edificia.Application.Tests.Auth;

public class UpdateProfileHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ILogger<UpdateProfileHandler>> _loggerMock;
    private readonly UpdateProfileHandler _handler;

    public UpdateProfileHandlerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _loggerMock = new Mock<ILogger<UpdateProfileHandler>>();

        _handler = new UpdateProfileHandler(
            _userManagerMock.Object,
            _loggerMock.Object);
    }

    private static ApplicationUser CreateTestUser(Guid? id = null)
    {
        return new ApplicationUser
        {
            Id = id ?? Guid.NewGuid(),
            Email = "test@edificia.dev",
            UserName = "test@edificia.dev",
            FullName = "Original Name",
            CollegiateNumber = "COL-001",
            IsActive = true,
            EmailConfirmed = true
        };
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenProfileIsUpdated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        var command = new UpdateProfileCommand(userId, "New Name", "COL-999");

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(userId);
        result.Value.Email.Should().Be("test@edificia.dev");
        result.Value.FullName.Should().Be("New Name");
        result.Value.CollegiateNumber.Should().Be("COL-999");
    }

    [Fact]
    public async Task Handle_ShouldUpdateUserFields_WhenSuccessful()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        var command = new UpdateProfileCommand(userId, "Updated Name", "COL-777");

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.FullName.Should().Be("Updated Name");
        user.CollegiateNumber.Should().Be("COL-777");
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldAllowNullCollegiateNumber()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        var command = new UpdateProfileCommand(userId, "New Name", null);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CollegiateNumber.Should().BeNull();
        user.CollegiateNumber.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateProfileCommand(userId, "New Name", null);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("InvalidCredentials");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUpdateFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        var command = new UpdateProfileCommand(userId, "New Name", "COL-999");

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Code = "ConcurrencyFailure", Description = "Conflict" }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("ProfileUpdateFailed");
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateAsync_WithCorrectUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        var command = new UpdateProfileCommand(userId, "New Name", "COL-555");

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }
}
