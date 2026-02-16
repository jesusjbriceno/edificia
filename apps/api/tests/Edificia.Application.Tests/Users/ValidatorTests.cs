using Edificia.Application.Users.Commands.CreateUser;
using Edificia.Application.Users.Commands.UpdateUser;
using Edificia.Application.Users.Queries.GetUsers;
using FluentValidation.TestHelper;

namespace Edificia.Application.Tests.Users;

public class CreateUserValidatorTests
{
    private readonly CreateUserValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenEmailIsEmpty()
    {
        var command = new CreateUserCommand("", "Test User", "Architect", null, Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_HaveError_WhenEmailIsInvalid()
    {
        var command = new CreateUserCommand("not-an-email", "Test User", "Architect", null, Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_HaveError_WhenFullNameIsEmpty()
    {
        var command = new CreateUserCommand("test@edificia.dev", "", "Architect", null, Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Should_HaveError_WhenFullNameExceedsMaxLength()
    {
        var longName = new string('A', 201);
        var command = new CreateUserCommand("test@edificia.dev", longName, "Architect", null, Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Should_HaveError_WhenRoleIsEmpty()
    {
        var command = new CreateUserCommand("test@edificia.dev", "Test User", "", null, Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Role);
    }

    [Theory]
    [InlineData("Root")]
    [InlineData("SuperAdmin")]
    [InlineData("Invalid")]
    public void Should_HaveError_WhenRoleIsNotAllowed(string role)
    {
        var command = new CreateUserCommand("test@edificia.dev", "Test User", role, null, Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Role);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Architect")]
    [InlineData("Collaborator")]
    public void Should_NotHaveRoleError_WhenRoleIsValid(string role)
    {
        var command = new CreateUserCommand("test@edificia.dev", "Test User", role, null, Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public void Should_HaveError_WhenCollegiateNumberExceedsMaxLength()
    {
        var longNumber = new string('0', 51);
        var command = new CreateUserCommand("test@edificia.dev", "Test User", "Architect", longNumber, Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CollegiateNumber);
    }

    [Fact]
    public void Should_HaveError_WhenCreatedByUserIdIsEmpty()
    {
        var command = new CreateUserCommand("test@edificia.dev", "Test User", "Architect", null, Guid.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CreatedByUserId);
    }

    [Fact]
    public void Should_NotHaveError_WhenCommandIsValid()
    {
        var command = new CreateUserCommand(
            "test@edificia.dev", "Test User", "Architect", "COL-001", Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_NotHaveError_WhenCollegiateNumberIsNull()
    {
        var command = new CreateUserCommand(
            "test@edificia.dev", "Test User", "Collaborator", null, Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class UpdateUserValidatorTests
{
    private readonly UpdateUserValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenUserIdIsEmpty()
    {
        var command = new UpdateUserCommand(Guid.Empty, "Test User", "Architect", null, Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Should_HaveError_WhenFullNameIsEmpty()
    {
        var command = new UpdateUserCommand(Guid.NewGuid(), "", "Architect", null, Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Should_HaveError_WhenRoleIsInvalid()
    {
        var command = new UpdateUserCommand(Guid.NewGuid(), "Test User", "Invalid", null, Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public void Should_HaveError_WhenUpdatedByUserIdIsEmpty()
    {
        var command = new UpdateUserCommand(Guid.NewGuid(), "Test User", "Architect", null, Guid.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UpdatedByUserId);
    }

    [Fact]
    public void Should_NotHaveError_WhenCommandIsValid()
    {
        var command = new UpdateUserCommand(
            Guid.NewGuid(), "Test User", "Architect", "COL-001", Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class GetUsersValidatorTests
{
    private readonly GetUsersValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenPageIsZero()
    {
        var query = new GetUsersQuery(Page: 0);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void Should_HaveError_WhenPageSizeIsZero()
    {
        var query = new GetUsersQuery(PageSize: 0);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Should_HaveError_WhenPageSizeExceedsMax()
    {
        var query = new GetUsersQuery(PageSize: 51);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Should_HaveError_WhenRoleIsInvalid()
    {
        var query = new GetUsersQuery(Role: "InvalidRole");
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Role);
    }

    [Theory]
    [InlineData("Root")]
    [InlineData("Admin")]
    [InlineData("Architect")]
    [InlineData("Collaborator")]
    public void Should_NotHaveRoleError_WhenRoleIsValid(string role)
    {
        var query = new GetUsersQuery(Role: role);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public void Should_NotHaveError_WhenQueryIsValid()
    {
        var query = new GetUsersQuery(Page: 1, PageSize: 10, Role: "Architect");
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_NotHaveError_WhenRoleIsNull()
    {
        var query = new GetUsersQuery(Role: null);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.Role);
    }
}
