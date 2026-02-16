using Edificia.Application.Auth.Commands.Login;
using Edificia.Application.Auth.Commands.ChangePassword;
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
