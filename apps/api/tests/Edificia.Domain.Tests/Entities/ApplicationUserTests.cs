using Edificia.Domain.Constants;
using Edificia.Domain.Entities;
using FluentAssertions;

namespace Edificia.Domain.Tests.Entities;

public class ApplicationUserTests
{
    [Fact]
    public void NewUser_ShouldHaveDefaultValues()
    {
        var user = new ApplicationUser();

        user.FullName.Should().Be(string.Empty);
        user.CollegiateNumber.Should().BeNull();
        user.MustChangePassword.Should().BeFalse();
        user.IsActive.Should().BeTrue();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void NewUser_ShouldAcceptFullNameAndCollegiateNumber()
    {
        var user = new ApplicationUser
        {
            FullName = "Juan García",
            CollegiateNumber = "COL-12345"
        };

        user.FullName.Should().Be("Juan García");
        user.CollegiateNumber.Should().Be("COL-12345");
    }

    [Fact]
    public void MustChangePassword_ShouldBeSettable()
    {
        var user = new ApplicationUser { MustChangePassword = true };

        user.MustChangePassword.Should().BeTrue();
    }

    [Fact]
    public void IsActive_ShouldBeSettable()
    {
        var user = new ApplicationUser { IsActive = false };

        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void NewUser_ShouldInheritFromIdentityUser()
    {
        var user = new ApplicationUser
        {
            Email = "test@edificia.dev",
            UserName = "test@edificia.dev"
        };

        user.Email.Should().Be("test@edificia.dev");
        user.UserName.Should().Be("test@edificia.dev");
        user.Id.Should().NotBe(Guid.Empty);
    }
}

public class AppRolesTests
{
    [Fact]
    public void All_ShouldContainFourRoles()
    {
        AppRoles.All.Should().HaveCount(4);
    }

    [Fact]
    public void All_ShouldContainExpectedRoles()
    {
        AppRoles.All.Should().Contain(AppRoles.Root);
        AppRoles.All.Should().Contain(AppRoles.Admin);
        AppRoles.All.Should().Contain(AppRoles.Architect);
        AppRoles.All.Should().Contain(AppRoles.Collaborator);
    }

    [Fact]
    public void RoleNames_ShouldHaveCorrectValues()
    {
        AppRoles.Root.Should().Be("Root");
        AppRoles.Admin.Should().Be("Admin");
        AppRoles.Architect.Should().Be("Architect");
        AppRoles.Collaborator.Should().Be("Collaborator");
    }
}

public class AppClaimsTests
{
    [Fact]
    public void AuthMethodReference_ShouldBeAmr()
    {
        AppClaims.AuthMethodReference.Should().Be("amr");
    }

    [Fact]
    public void PasswordChangeRequired_ShouldHaveCorrectValue()
    {
        AppClaims.PasswordChangeRequired.Should().Be("pwd_change_required");
    }
}

public class AppPoliciesTests
{
    [Fact]
    public void PolicyNames_ShouldHaveCorrectValues()
    {
        AppPolicies.ActiveUser.Should().Be("ActiveUser");
        AppPolicies.RequireRoot.Should().Be("RequireRoot");
        AppPolicies.RequireAdmin.Should().Be("RequireAdmin");
        AppPolicies.RequireArchitect.Should().Be("RequireArchitect");
    }
}
