using Edificia.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Edificia.Infrastructure.TemplateStorage;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly TemplateStorageSettings _settings;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(
        IOptions<TemplateStorageSettings> settings,
        ILogger<LocalFileStorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_settings.BasePath))
        {
            throw new InvalidOperationException("TemplateStorage:BasePath es obligatorio para proveedor local.");
        }

        Directory.CreateDirectory(_settings.BasePath);
    }

    public async Task<string> SaveFileAsync(
        Stream fileStream,
        string fileName,
        string templateType,
        CancellationToken cancellationToken = default)
    {
        if (fileStream is null)
            throw new ArgumentNullException(nameof(fileStream));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("FileName es obligatorio.", nameof(fileName));

        var safeTemplateType = SanitizePathPart(templateType);
        var fileExtension = Path.GetExtension(fileName);
        var generatedName = $"{Guid.NewGuid():N}{fileExtension}";
        var datePath = DateTime.UtcNow.ToString("yyyy/MM");
        var relativePath = Path.Combine(safeTemplateType, datePath, generatedName)
            .Replace('\\', '/');

        var fullPath = Path.Combine(
            _settings.BasePath,
            relativePath.Replace('/', Path.DirectorySeparatorChar));

        var directory = Path.GetDirectoryName(fullPath)
            ?? throw new InvalidOperationException("No se pudo resolver el directorio de destino.");

        Directory.CreateDirectory(directory);

        if (fileStream.CanSeek)
            fileStream.Position = 0;

        await using var output = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await fileStream.CopyToAsync(output, cancellationToken);

        _logger.LogInformation("Template saved in local storage: {RelativePath}", relativePath);

        return relativePath;
    }

    public async Task<byte[]> GetFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new ArgumentException("RelativePath es obligatorio.", nameof(relativePath));

        var fullPath = ResolveFullPath(relativePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("No se encontró el archivo de plantilla.", fullPath);

        return await File.ReadAllBytesAsync(fullPath, cancellationToken);
    }

    public Task<bool> DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new ArgumentException("RelativePath es obligatorio.", nameof(relativePath));

        var fullPath = ResolveFullPath(relativePath);
        if (!File.Exists(fullPath))
            return Task.FromResult(false);

        File.Delete(fullPath);
        _logger.LogInformation("Template deleted from local storage: {RelativePath}", relativePath);

        return Task.FromResult(true);
    }

    private string ResolveFullPath(string relativePath)
    {
        return Path.Combine(
            _settings.BasePath,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    private static string SanitizePathPart(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "default";

        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(value
            .Trim()
            .Where(ch => !invalidChars.Contains(ch))
            .ToArray());

        return string.IsNullOrWhiteSpace(sanitized)
            ? "default"
            : sanitized;
    }
}
