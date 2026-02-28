namespace Edificia.Application.Interfaces;

/// <summary>
/// Abstraction for template binary storage.
/// Implementations can delegate to local filesystem or external providers (e.g., n8n workflows).
/// </summary>
public interface IFileStorageService
{
    Task<string> SaveFileAsync(
        Stream fileStream,
        string fileName,
        string templateType,
        CancellationToken cancellationToken = default);

    Task<byte[]> GetFileAsync(
        string relativePath,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteFileAsync(
        string relativePath,
        CancellationToken cancellationToken = default);
}
