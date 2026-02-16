using Edificia.Application.Auth.Commands.Login;
using Edificia.Application.Auth.Commands.ChangePassword;
using Edificia.Application.Auth.Commands.UpdateProfile;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Edificia.Application.Tests.Auth;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenEmailIsEmpty()
    {
        var result = _validator.TestValidate(new LoginCommand("", "Password123!"));
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_HaveError_WhenEmailIsInvalid()
    {
        var result = _validator.TestValidate(new LoginCommand("not-an-email", "Password123!"));
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_HaveError_WhenPasswordIsEmpty()
    {
        var result = _validator.TestValidate(new LoginCommand("test@email.com", ""));
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_NotHaveError_WhenCommandIsValid()
    {
        var result = _validator.TestValidate(new LoginCommand("test@email.com", "Password123!"));
        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class ChangePasswordValidatorTests
{
    private readonly ChangePasswordValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenCurrentPasswordIsEmpty()
    {
        var command = new ChangePasswordCommand(Guid.NewGuid(), "", "NewPass123!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword);
    }

    [Fact]
    public void Should_HaveError_WhenNewPasswordIsTooShort()
    {
        var command = new ChangePasswordCommand(Guid.NewGuid(), "OldPass!", "Ab1!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Should_HaveError_WhenNewPasswordLacksUppercase()
    {
        var command = new ChangePasswordCommand(Guid.NewGuid(), "OldPass!", "newpass123!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Should_HaveError_WhenNewPasswordLacksLowercase()
    {
        var command = new ChangePasswordCommand(Guid.NewGuid(), "OldPass!", "NEWPASS123!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Should_HaveError_WhenNewPasswordLacksDigit()
    {
        var command = new ChangePasswordCommand(Guid.NewGuid(), "OldPass!", "NewPassWord!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Should_HaveError_WhenNewPasswordLacksSpecialChar()
    {
        var command = new ChangePasswordCommand(Guid.NewGuid(), "OldPass!", "NewPass1234");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Should_HaveError_WhenNewPasswordEqualsCurrentPassword()
    {
        var command = new ChangePasswordCommand(Guid.NewGuid(), "SamePass123!", "SamePass123!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Should_NotHaveError_WhenCommandIsValid()
    {
        var command = new ChangePasswordCommand(Guid.NewGuid(), "OldPass123!", "NewPass456!");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class UpdateProfileValidatorTests
{
    private readonly UpdateProfileValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenUserIdIsEmpty()
    {
        var command = new UpdateProfileCommand(Guid.Empty, "Name", null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Should_HaveError_WhenFullNameIsEmpty()
    {
        var command = new UpdateProfileCommand(Guid.NewGuid(), "", null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Should_HaveError_WhenFullNameExceedsMaxLength()
    {
        var command = new UpdateProfileCommand(Guid.NewGuid(), new string('A', 201), null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Should_HaveError_WhenCollegiateNumberExceedsMaxLength()
    {
        var command = new UpdateProfileCommand(Guid.NewGuid(), "Name", new string('X', 51));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CollegiateNumber);
    }

    [Fact]
    public void Should_NotHaveError_WhenCollegiateNumberIsNull()
    {
        var command = new UpdateProfileCommand(Guid.NewGuid(), "Valid Name", null);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_NotHaveError_WhenCommandIsValid()
    {
        var command = new UpdateProfileCommand(Guid.NewGuid(), "Valid Name", "COL-123");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
