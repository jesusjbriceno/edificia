using Edificia.Domain.Entities;
using Edificia.Infrastructure.Persistence;
using Edificia.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Edificia.Application.Tests.Templates.Repositories;

public sealed class TemplateRepositoryTests
{
    [Fact]
    public async Task GetDefaultByTypeAsync_ShouldReturnDefaultTemplate()
    {
        await using var context = CreateContext();
        var repository = new TemplateRepository(context);

        var defaultTemplate = CreateTemplate("Plantilla v2", "MemoriaTecnica", version: 2, isAvailable: true, isDefault: true);
        var availableTemplate = CreateTemplate("Plantilla v1", "MemoriaTecnica", version: 1, isAvailable: true, isDefault: false);

        await context.Templates.AddRangeAsync(defaultTemplate, availableTemplate);
        await context.SaveChangesAsync();

        var result = await repository.GetDefaultByTypeAsync("MemoriaTecnica");

        result.Should().NotBeNull();
        result!.IsDefault.Should().BeTrue();
        result.Version.Should().Be(2);
    }

    [Fact]
    public async Task GetAvailableByTypeAsync_ShouldReturnOnlyAvailableTemplatesOrderedByVersion()
    {
        await using var context = CreateContext();
        var repository = new TemplateRepository(context);

        var availableV3 = CreateTemplate("Plantilla v3", "MemoriaTecnica", version: 3, isAvailable: true, isDefault: false);
        var availableV1 = CreateTemplate("Plantilla v1", "MemoriaTecnica", version: 1, isAvailable: true, isDefault: false);
        var unavailableV2 = CreateTemplate("Plantilla v2", "MemoriaTecnica", version: 2, isAvailable: false, isDefault: false);

        await context.Templates.AddRangeAsync(availableV3, availableV1, unavailableV2);
        await context.SaveChangesAsync();

        var result = await repository.GetAvailableByTypeAsync("MemoriaTecnica");

        result.Should().HaveCount(2);
        result.Select(t => t.Version).Should().ContainInOrder(3, 1);
        result.Should().OnlyContain(t => t.IsAvailable);
    }

    [Fact]
    public async Task GetActiveByTypeAsync_ShouldResolveFromAvailableTemplates()
    {
        await using var context = CreateContext();
        var repository = new TemplateRepository(context);

        var unavailableTemplate = CreateTemplate("Plantilla no disponible", "MemoriaTecnica", version: 4, isAvailable: false, isDefault: false);
        var availableTemplate = CreateTemplate("Plantilla disponible", "MemoriaTecnica", version: 2, isAvailable: true, isDefault: false);

        await context.Templates.AddRangeAsync(unavailableTemplate, availableTemplate);
        await context.SaveChangesAsync();

        var result = await repository.GetActiveByTypeAsync("MemoriaTecnica");

        result.Should().NotBeNull();
        result!.Name.Should().Be("Plantilla disponible");
        result.IsActive.Should().BeTrue();
    }

    private static EdificiaDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EdificiaDbContext>()
            .UseInMemoryDatabase($"templates-repository-{Guid.NewGuid():N}")
            .Options;

        return new EdificiaDbContext(options);
    }

    private static AppTemplate CreateTemplate(
        string name,
        string templateType,
        int version,
        bool isAvailable,
        bool isDefault)
    {
        var template = AppTemplate.Create(
            name: name,
            description: "Template test",
            templateType: templateType,
            storagePath: $"templates/{templateType}/v1.dotx",
            originalFileName: "v1.dotx",
            mimeType: "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
            fileSizeBytes: 1024,
            createdByUserId: Guid.NewGuid());

        while (template.Version < version)
        {
            template.PublishNewVersion(
                storagePath: $"templates/{templateType}/v{template.Version + 1}.dotx",
                originalFileName: $"v{template.Version + 1}.dotx",
                mimeType: "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
                fileSizeBytes: 1024);
        }

        template.SetAvailable(isAvailable);
        if (isDefault)
        {
            if (!template.IsAvailable)
            {
                template.SetAvailable(true);
            }

            template.MarkAsDefault();
        }

        return template;
    }
}