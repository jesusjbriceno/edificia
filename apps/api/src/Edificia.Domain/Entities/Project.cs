using Edificia.Domain.Enums;
using Edificia.Domain.Primitives;

namespace Edificia.Domain.Entities;

/// <summary>
/// Represents a construction project with its intervention strategy and normative content.
/// Core aggregate root of the system.
/// </summary>
public sealed class Project : AuditableEntity
{
    /// <summary>Nombre descriptivo del proyecto.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Descripción opcional del proyecto.</summary>
    public string? Description { get; private set; }

    /// <summary>Dirección de la obra.</summary>
    public string? Address { get; private set; }

    /// <summary>Tipo de intervención (Obra Nueva, Reforma, Ampliación).</summary>
    public InterventionType InterventionType { get; private set; }

    /// <summary>
    /// Indica si la obra está sujeta a LOE (Art 4) o exenta (Art 2.2).
    /// true = sujeta a LOE (normativa completa), false = exenta.
    /// </summary>
    public bool IsLoeRequired { get; private set; }

    /// <summary>Referencia catastral de la parcela (opcional).</summary>
    public string? CadastralReference { get; private set; }

    /// <summary>Normativa municipal específica (ordenanzas locales).</summary>
    public string? LocalRegulations { get; private set; }

    /// <summary>Estado del proyecto en el flujo de trabajo.</summary>
    public ProjectStatus Status { get; private set; }

    /// <summary>
    /// Árbol de contenido de la memoria en formato JSON (JSONB en PostgreSQL).
    /// Almacena la estructura completa de capítulos y secciones.
    /// </summary>
    public string? ContentTreeJson { get; private set; }

    // EF Core requires parameterless constructor
    private Project() { }

    /// <summary>
    /// Creates a new project with the required configuration.
    /// </summary>
    public static Project Create(
        string title,
        InterventionType interventionType,
        bool isLoeRequired,
        string? description = null,
        string? address = null,
        string? cadastralReference = null,
        string? localRegulations = null)
    {
        return new Project(Guid.NewGuid())
        {
            Title = title,
            InterventionType = interventionType,
            IsLoeRequired = isLoeRequired,
            Description = description,
            Address = address,
            CadastralReference = cadastralReference,
            LocalRegulations = localRegulations,
            Status = ProjectStatus.Draft
        };
    }

    private Project(Guid id) : base(id) { }

    /// <summary>Updates the project settings (strategy configuration).</summary>
    public void UpdateSettings(
        string title,
        InterventionType interventionType,
        bool isLoeRequired,
        string? description = null,
        string? address = null,
        string? cadastralReference = null,
        string? localRegulations = null)
    {
        Title = title;
        InterventionType = interventionType;
        IsLoeRequired = isLoeRequired;
        Description = description;
        Address = address;
        CadastralReference = cadastralReference;
        LocalRegulations = localRegulations;
    }

    /// <summary>Advances the project to InProgress status.</summary>
    public void StartRedaction()
    {
        Status = ProjectStatus.InProgress;
    }

    /// <summary>Marks the project as completed.</summary>
    public void Complete()
    {
        Status = ProjectStatus.Completed;
    }

    /// <summary>Archives the project.</summary>
    public void Archive()
    {
        Status = ProjectStatus.Archived;
    }

    /// <summary>Updates the content tree JSON (JSONB stored).</summary>
    public void UpdateContentTree(string contentTreeJson)
    {
        ContentTreeJson = contentTreeJson;
    }
}
