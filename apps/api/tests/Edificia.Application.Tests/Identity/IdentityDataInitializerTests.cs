using Edificia.Domain.Constants;
using Edificia.Domain.Entities;
using Edificia.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Edificia.Application.Tests.Identity;

public class IdentityDataInitializerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole<Guid>>> _roleManagerMock;
    private readonly SecuritySettings _settings;
    private readonly IdentityDataInitializer _initializer;

    public IdentityDataInitializerTests()
    {
        // UserManager mock setup
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        // RoleManager mock setup
        var roleStore = new Mock<IRoleStore<IdentityRole<Guid>>>();
        _roleManagerMock = new Mock<RoleManager<IdentityRole<Guid>>>(
            roleStore.Object, null!, null!, null!, null!);

        _settings = new SecuritySettings
        {
            RootEmail = "root@test.dev",
            RootInitialPassword = "TestPassword123!"
        };

        // Build a service provider with the mocked services
        var services = new ServiceCollection();
        services.AddSingleton(_userManagerMock.Object);
        services.AddSingleton(_roleManagerMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        _initializer = new IdentityDataInitializer(
            serviceProvider,
            Options.Create(_settings),
            NullLogger<IdentityDataInitializer>.Instance);
    }

    [Fact]
    public async Task StartAsync_ShouldCreateAllRoles_WhenNoneExist()
    {
        // Arrange: no roles exist
        _roleManagerMock
            .Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _roleManagerMock
            .Setup(r => r.CreateAsync(It.IsAny<IdentityRole<Guid>>()))
            .ReturnsAsync(IdentityResult.Success);

        // No root users exist
        _userManagerMock
            .Setup(u => u.GetUsersInRoleAsync(AppRoles.Root))
            .ReturnsAsync(new List<ApplicationUser>());
        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), AppRoles.Root))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _initializer.StartAsync(CancellationToken.None);

        // Assert: all 4 roles created
        _roleManagerMock.Verify(
            r => r.CreateAsync(It.IsAny<IdentityRole<Guid>>()),
            Times.Exactly(AppRoles.All.Length));
    }

    [Fact]
    public async Task StartAsync_ShouldNotCreateRoles_WhenAlreadyExist()
    {
        _roleManagerMock
            .Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(u => u.GetUsersInRoleAsync(AppRoles.Root))
            .ReturnsAsync(new List<ApplicationUser> { new() });

        await _initializer.StartAsync(CancellationToken.None);

        _roleManagerMock.Verify(
            r => r.CreateAsync(It.IsAny<IdentityRole<Guid>>()),
            Times.Never);
    }

    [Fact]
    public async Task StartAsync_ShouldCreateRootUser_WhenNoneExists()
    {
        SetupRolesExist();

        _userManagerMock
            .Setup(u => u.GetUsersInRoleAsync(AppRoles.Root))
            .ReturnsAsync(new List<ApplicationUser>());
        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), _settings.RootInitialPassword))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), AppRoles.Root))
            .ReturnsAsync(IdentityResult.Success);

        await _initializer.StartAsync(CancellationToken.None);

        _userManagerMock.Verify(
            u => u.CreateAsync(
                It.Is<ApplicationUser>(user =>
                    user.Email == _settings.RootEmail &&
                    user.UserName == _settings.RootEmail &&
                    user.MustChangePassword == true &&
                    user.EmailConfirmed == true &&
                    user.FullName == "Administrador del Sistema"),
                _settings.RootInitialPassword),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_ShouldAssignRootRole_AfterCreatingUser()
    {
        SetupRolesExist();

        _userManagerMock
            .Setup(u => u.GetUsersInRoleAsync(AppRoles.Root))
            .ReturnsAsync(new List<ApplicationUser>());
        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), AppRoles.Root))
            .ReturnsAsync(IdentityResult.Success);

        await _initializer.StartAsync(CancellationToken.None);

        _userManagerMock.Verify(
            u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), AppRoles.Root),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_ShouldSkipRootUser_WhenAlreadyExists()
    {
        SetupRolesExist();

        _userManagerMock
            .Setup(u => u.GetUsersInRoleAsync(AppRoles.Root))
            .ReturnsAsync(new List<ApplicationUser> { new() { Email = "existing@test.dev" } });

        await _initializer.StartAsync(CancellationToken.None);

        _userManagerMock.Verify(
            u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task StartAsync_ShouldNotAssignRole_WhenUserCreationFails()
    {
        SetupRolesExist();

        _userManagerMock
            .Setup(u => u.GetUsersInRoleAsync(AppRoles.Root))
            .ReturnsAsync(new List<ApplicationUser>());
        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordTooWeak",
                Description = "Password does not meet requirements"
            }));

        await _initializer.StartAsync(CancellationToken.None);

        _userManagerMock.Verify(
            u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task StopAsync_ShouldCompleteImmediately()
    {
        var task = _initializer.StopAsync(CancellationToken.None);

        task.IsCompleted.Should().BeTrue();
    }

    // ── Helpers ──

    private void SetupRolesExist()
    {
        _roleManagerMock
            .Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
    }
}
