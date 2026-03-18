using Edificia.Application.Templates.Commands.SetTemplateDefault;
using Edificia.Application.Templates.DTOs;
using FluentAssertions;

namespace Edificia.Application.Tests.Templates.Commands;

public class SetTemplateDefaultMappingTests
{
    [Fact]
    public void CreateFactory_ShouldMapFields()
    {
        var templateId = Guid.NewGuid();
        var request = new SetTemplateDefaultRequest(true);

        var command = SetTemplateDefaultCommand.Create(templateId, request);

        command.TemplateId.Should().Be(templateId);
        command.IsDefault.Should().BeTrue();
    }
}