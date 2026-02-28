using Edificia.Infrastructure.TemplateStorage;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Edificia.Application.Tests.TemplateStorage;

public class LocalFileStorageServiceTests
{
    [Fact]
    public async Task Save_Get_Delete_ShouldWorkWithLocalProvider()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), "edificia-tests", Guid.NewGuid().ToString("N"));

        var settings = Options.Create(new TemplateStorageSettings
        {
            Provider = "local",
            BasePath = tempPath
        });

        var service = new LocalFileStorageService(settings, Mock.Of<ILogger<LocalFileStorageService>>());
        var inputBytes = new byte[] { 1, 2, 3, 4, 5, 6 };

        await using var stream = new MemoryStream(inputBytes);
        var relativePath = await service.SaveFileAsync(stream, "plantilla.dotx", "MemoriaTecnica");

        relativePath.Should().Contain("MemoriaTecnica");

        var outputBytes = await service.GetFileAsync(relativePath);
        outputBytes.Should().Equal(inputBytes);

        var deleted = await service.DeleteFileAsync(relativePath);
        deleted.Should().BeTrue();

        var deletedAgain = await service.DeleteFileAsync(relativePath);
        deletedAgain.Should().BeFalse();

        Directory.Delete(tempPath, recursive: true);
    }
}
