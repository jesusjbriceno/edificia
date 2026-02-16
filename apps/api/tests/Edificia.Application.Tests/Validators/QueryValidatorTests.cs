using Edificia.Application.Projects.Queries.GetProjectById;
using Edificia.Application.Projects.Queries.GetProjectTree;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Edificia.Application.Tests.Validators;

public class QueryValidatorTests
{
    // ---------- GetProjectByIdValidator ----------

    [Fact]
    public void GetProjectById_WithValidId_ShouldNotHaveErrors()
    {
        var validator = new GetProjectByIdValidator();
        var query = new GetProjectByIdQuery(Guid.NewGuid());

        var result = validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void GetProjectById_WithEmptyId_ShouldHaveError()
    {
        var validator = new GetProjectByIdValidator();
        var query = new GetProjectByIdQuery(Guid.Empty);

        var result = validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    // ---------- GetProjectTreeValidator ----------

    [Fact]
    public void GetProjectTree_WithValidId_ShouldNotHaveErrors()
    {
        var validator = new GetProjectTreeValidator();
        var query = new GetProjectTreeQuery(Guid.NewGuid());

        var result = validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void GetProjectTree_WithEmptyId_ShouldHaveError()
    {
        var validator = new GetProjectTreeValidator();
        var query = new GetProjectTreeQuery(Guid.Empty);

        var result = validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.ProjectId);
    }
}
