using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Edificia.Domain.Enums;
using Edificia.Domain.Exceptions;
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

    /// <summary>ID del usuario que creó el proyecto (FK a AspNetUsers).</summary>
    public Guid CreatedByUserId { get; private set; }

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
        Guid createdByUserId,
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
            CreatedByUserId = createdByUserId,
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
        if (Status is not ProjectStatus.Draft)
            throw new BusinessRuleException(
                "Project.InvalidTransition",
                "Solo se puede iniciar la redacción desde el estado borrador.");

        Status = ProjectStatus.InProgress;
    }

    /// <summary>
    /// Envía el proyecto a revisión por un administrador.
    /// Solo permitido desde Draft o InProgress.
    /// </summary>
    public void SubmitForReview()
    {
        if (Status == ProjectStatus.PendingReview)
            throw new BusinessRuleException(
                "Project.AlreadyPendingReview",
                "El proyecto ya está pendiente de revisión.");

        if (Status is not (ProjectStatus.Draft or ProjectStatus.InProgress))
            throw new BusinessRuleException(
                "Project.InvalidTransition",
                $"El proyecto en estado '{Status}' no se puede enviar a revisión. Solo es posible desde Borrador o En Redacción.");

        Status = ProjectStatus.PendingReview;
    }

    /// <summary>
    /// Aprueba el proyecto pendiente de revisión, marcándolo como Completado.
    /// Solo permitido desde PendingReview.
    /// </summary>
    public void Approve()
    {
        if (Status is not ProjectStatus.PendingReview)
            throw new BusinessRuleException(
                "Project.InvalidTransition",
                "El proyecto solo se puede aprobar cuando está pendiente de revisión.");

        Status = ProjectStatus.Completed;
    }

    /// <summary>
    /// Marks the project as completed. Only allowed from PendingReview.
    /// </summary>
    public void Complete()
    {
        if (Status is not ProjectStatus.PendingReview)
            throw new BusinessRuleException(
                "Project.InvalidTransition",
                "El proyecto solo se puede completar cuando está pendiente de revisión.");

        Status = ProjectStatus.Completed;
    }

    /// <summary>
    /// Rechaza el proyecto pendiente de revisión, devolviéndolo a Draft.
    /// Solo permitido desde PendingReview.
    /// </summary>
    public void Reject()
    {
        if (Status is not ProjectStatus.PendingReview)
            throw new BusinessRuleException(
                "Project.InvalidTransition",
                "El proyecto solo se puede rechazar cuando está pendiente de revisión.");

        Status = ProjectStatus.Draft;
    }

    /// <summary>Archives the project. Only allowed from Completed.</summary>
    public void Archive()
    {
        if (Status == ProjectStatus.Archived)
            throw new BusinessRuleException(
                "Project.AlreadyArchived",
                "El proyecto ya está archivado.");

        if (Status is not ProjectStatus.Completed)
            throw new BusinessRuleException(
                "Project.InvalidTransition",
                "El proyecto solo se puede archivar cuando está completado.");

        Status = ProjectStatus.Archived;
    }

    /// <summary>Updates the content tree JSON (JSONB stored).</summary>
    public void UpdateContentTree(string contentTreeJson)
    {
        EnsureEditable();
        ContentTreeJson = contentTreeJson;
    }

    /// <summary>
    /// Updates the content of a specific section within the content tree.
    /// Recursively searches chapters and nested sections by ID.
    /// </summary>
    /// <param name="sectionId">The unique identifier of the section to update.</param>
    /// <param name="content">The new content for the section.</param>
    /// <returns>True if the section was found and updated; false otherwise.</returns>
    public bool UpdateSectionContent(string sectionId, string content)
    {
        EnsureEditable();

        if (ContentTreeJson is null)
            return false;

        var root = JsonNode.Parse(ContentTreeJson);
        var chapters = root?["chapters"]?.AsArray();

        if (chapters is null || chapters.Count == 0)
            return false;

        foreach (var chapter in chapters)
        {
            if (chapter is not null && UpdateNodeContent(chapter, sectionId, content))
            {
                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                ContentTreeJson = root!.ToJsonString(options);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Validates that the project is in an editable state (Draft or InProgress).
    /// Throws BusinessRuleException if the project is readonly.
    /// </summary>
    private void EnsureEditable()
    {
        if (Status == ProjectStatus.PendingReview)
            throw new BusinessRuleException(
                "Project.ReadOnly",
                "El proyecto no se puede editar mientras está pendiente de revisión.");

        if (Status == ProjectStatus.Completed)
            throw new BusinessRuleException(
                "Project.ReadOnly",
                "El proyecto no se puede editar porque está completado.");

        if (Status == ProjectStatus.Archived)
            throw new BusinessRuleException(
                "Project.ReadOnly",
                "El proyecto no se puede editar porque está archivado.");
    }

    private static bool UpdateNodeContent(JsonNode node, string sectionId, string content)
    {
        if (node["id"]?.GetValue<string>() == sectionId)
        {
            node["content"] = content;
            return true;
        }

        var sections = node["sections"]?.AsArray();
        if (sections is not null)
        {
            foreach (var section in sections)
            {
                if (section is not null && UpdateNodeContent(section, sectionId, content))
                    return true;
            }
        }

        return false;
    }
}
