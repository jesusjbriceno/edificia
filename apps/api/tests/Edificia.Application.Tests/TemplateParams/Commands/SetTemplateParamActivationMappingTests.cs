using Edificia.Application.TemplateParams.Commands.SetTemplateParamActivation;
using Edificia.Application.TemplateParams.DTOs;
using FluentAssertions;

namespace Edificia.Application.Tests.TemplateParams.Commands;

public class SetTemplateParamActivationMappingTests
{
    [Fact]
    public void CreateFactory_ShouldMapFields()
    {
        var paramId = Guid.NewGuid();
        var request = new SetTemplateParamActivationRequest(true);

        var command = SetTemplateParamActivationCommand.Create(paramId, request);

        command.TemplateParamId.Should().Be(paramId);
        command.IsActive.Should().BeTrue();
    }
}
