using Edificia.Domain.Exceptions;
using FluentAssertions;

namespace Edificia.Domain.Tests.Exceptions;

public class DomainExceptionTests
{
    [Fact]
    public void EntityNotFoundException_ShouldContainEntityInfo()
    {
        var id = Guid.NewGuid();
        var exception = new EntityNotFoundException("Project", id);

        exception.Code.Should().Be("Domain.EntityNotFound");
        exception.EntityName.Should().Be("Project");
        exception.EntityId.Should().Be(id);
        exception.Message.Should().Contain("Project");
        exception.Message.Should().Contain(id.ToString());
    }

    [Fact]
    public void BusinessRuleException_ShouldContainCodeAndMessage()
    {
        var exception = new BusinessRuleException(
            "Project.InvalidState",
            "Un proyecto archivado no puede ser editado.");

        exception.Code.Should().Be("Project.InvalidState");
        exception.Message.Should().Contain("archivado");
    }
}
