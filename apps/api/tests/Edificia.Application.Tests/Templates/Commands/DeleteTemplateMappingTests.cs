using Edificia.Application.Templates.Commands.DeleteTemplate;
using FluentAssertions;

namespace Edificia.Application.Tests.Templates.Commands;

public class DeleteTemplateMappingTests
{
    [Fact]
    public void Ctor_ShouldMapTemplateId()
    {
        var templateId = Guid.NewGuid();

        var command = new DeleteTemplateCommand(templateId);

        command.TemplateId.Should().Be(templateId);
    }
}