using Edificia.Domain.Exceptions;
using Edificia.Domain.Primitives;

namespace Edificia.Domain.Entities;

/// <summary>
/// Template metadata aggregate for .dotx document templates.
/// Binary content is stored externally (n8n/local provider) and referenced by StoragePath.
/// </summary>
public sealed class AppTemplate : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string TemplateType { get; private set; } = string.Empty;
    public string StoragePath { get; private set; } = string.Empty;
    public string OriginalFileName { get; private set; } = string.Empty;
    public string MimeType { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public bool IsActive { get; private set; }
    public int Version { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    private AppTemplate() { }

    private AppTemplate(Guid id) : base(id) { }

    public static AppTemplate Create(
        string name,
        string? description,
        string templateType,
        string storagePath,
        string originalFileName,
        string mimeType,
        long fileSizeBytes,
        Guid createdByUserId)
    {
        EnsureRequired(name, nameof(name));
        EnsureRequired(templateType, nameof(templateType));
        EnsureRequired(storagePath, nameof(storagePath));
        EnsureRequired(originalFileName, nameof(originalFileName));
        EnsureRequired(mimeType, nameof(mimeType));

        if (fileSizeBytes <= 0)
        {
            throw new BusinessRuleException(
                "Template.InvalidFileSize",
                "El tamaño del archivo de la plantilla debe ser mayor que cero.");
        }

        if (createdByUserId == Guid.Empty)
        {
            throw new BusinessRuleException(
                "Template.InvalidCreator",
                "La plantilla debe tener un usuario creador válido.");
        }

        return new AppTemplate(Guid.NewGuid())
        {
            Name = name.Trim(),
            Description = NormalizeDescription(description),
            TemplateType = templateType.Trim(),
            StoragePath = storagePath.Trim(),
            OriginalFileName = originalFileName.Trim(),
            MimeType = mimeType.Trim(),
            FileSizeBytes = fileSizeBytes,
            IsActive = false,
            Version = 1,
            CreatedByUserId = createdByUserId
        };
    }

    public void Activate()
    {
        if (IsActive)
        {
            throw new BusinessRuleException(
                "Template.AlreadyActive",
                "La plantilla ya está activa.");
        }

        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void PublishNewVersion(
        string storagePath,
        string originalFileName,
        string mimeType,
        long fileSizeBytes)
    {
        EnsureRequired(storagePath, nameof(storagePath));
        EnsureRequired(originalFileName, nameof(originalFileName));
        EnsureRequired(mimeType, nameof(mimeType));

        if (fileSizeBytes <= 0)
        {
            throw new BusinessRuleException(
                "Template.InvalidFileSize",
                "El tamaño del archivo de la plantilla debe ser mayor que cero.");
        }

        StoragePath = storagePath.Trim();
        OriginalFileName = originalFileName.Trim();
        MimeType = mimeType.Trim();
        FileSizeBytes = fileSizeBytes;
        Version++;
        IsActive = true;
    }

    public void Rename(string name, string? description)
    {
        EnsureRequired(name, nameof(name));

        Name = name.Trim();
        Description = NormalizeDescription(description);
    }

    private static void EnsureRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BusinessRuleException(
                "Template.InvalidField",
                $"El campo '{fieldName}' es obligatorio.");
        }
    }

    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }
}
