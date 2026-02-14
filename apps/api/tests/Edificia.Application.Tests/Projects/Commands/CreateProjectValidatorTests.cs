using Edificia.Application.Projects.Commands.CreateProject;
using Edificia.Domain.Enums;
using FluentAssertions;

namespace Edificia.Application.Tests.Projects.Commands;

public class CreateProjectValidatorTests
{
    private readonly CreateProjectValidator _validator = new();

    [Fact]
    public void ShouldPass_WhenAllFieldsAreValid()
    {
        var command = new CreateProjectCommand(
            "Vivienda Unifamiliar",
            InterventionType.NewConstruction,
            true,
            "Descripción del proyecto",
            "Calle Mayor 1, Madrid");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldPass_WithOnlyRequiredFields()
    {
        var command = new CreateProjectCommand(
            "Reforma local",
            InterventionType.Reform,
            false);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldFail_WhenTitleIsEmpty()
    {
        var command = new CreateProjectCommand(
            "",
            InterventionType.NewConstruction,
            true);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Title" &&
            e.ErrorMessage.Contains("obligatorio"));
    }

    [Fact]
    public void ShouldFail_WhenTitleExceedsMaxLength()
    {
        var command = new CreateProjectCommand(
            new string('A', 301),
            InterventionType.NewConstruction,
            true);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Title" &&
            e.ErrorMessage.Contains("300"));
    }

    [Fact]
    public void ShouldFail_WhenInterventionTypeIsInvalid()
    {
        var command = new CreateProjectCommand(
            "Test",
            (InterventionType)99,
            true);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "InterventionType");
    }

    [Fact]
    public void ShouldFail_WhenDescriptionExceedsMaxLength()
    {
        var command = new CreateProjectCommand(
            "Test",
            InterventionType.Reform,
            false,
            Description: new string('X', 2001));

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Description" &&
            e.ErrorMessage.Contains("2000"));
    }

    [Fact]
    public void ShouldFail_WhenAddressExceedsMaxLength()
    {
        var command = new CreateProjectCommand(
            "Test",
            InterventionType.NewConstruction,
            true,
            Address: new string('X', 501));

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Address" &&
            e.ErrorMessage.Contains("500"));
    }

    [Fact]
    public void ShouldFail_WhenCadastralReferenceExceedsMaxLength()
    {
        var command = new CreateProjectCommand(
            "Test",
            InterventionType.NewConstruction,
            true,
            CadastralReference: new string('X', 101));

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "CadastralReference" &&
            e.ErrorMessage.Contains("100"));
    }

    [Fact]
    public void ShouldFail_WhenLocalRegulationsExceedsMaxLength()
    {
        var command = new CreateProjectCommand(
            "Test",
            InterventionType.NewConstruction,
            true,
            LocalRegulations: new string('X', 5001));

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "LocalRegulations" &&
            e.ErrorMessage.Contains("5000"));
    }

    [Fact]
    public void ShouldPass_WhenOptionalFieldsAreNull()
    {
        var command = new CreateProjectCommand(
            "Test",
            InterventionType.Extension,
            true,
            null, null, null, null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
