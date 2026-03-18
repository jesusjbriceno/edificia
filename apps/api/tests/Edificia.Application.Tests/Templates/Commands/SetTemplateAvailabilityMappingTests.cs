using Edificia.Application.Templates.Commands.SetTemplateAvailability;
using Edificia.Application.Templates.DTOs;
using FluentAssertions;

namespace Edificia.Application.Tests.Templates.Commands;

public class SetTemplateAvailabilityMappingTests
{
    [Fact]
    public void CreateFactory_ShouldMapFields()
    {
        var templateId = Guid.NewGuid();
        var request = new SetTemplateAvailabilityRequest(true);

        var command = SetTemplateAvailabilityCommand.Create(templateId, request);

        command.TemplateId.Should().Be(templateId);
        command.IsAvailable.Should().BeTrue();
    }
}