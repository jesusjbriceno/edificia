using Edificia.Application.Templates.Commands.ToggleTemplateStatus;
using Edificia.Application.Templates.DTOs;
using FluentAssertions;

namespace Edificia.Application.Tests.Templates.Commands;

public class ToggleTemplateStatusMappingTests
{
    [Fact]
    public void CreateFactory_ShouldMapFields()
    {
        var id = Guid.NewGuid();
        var request = new ToggleTemplateStatusRequest(true);

        var command = ToggleTemplateStatusCommand.Create(id, request);

        command.TemplateId.Should().Be(id);
        command.IsActive.Should().BeTrue();
    }
}
