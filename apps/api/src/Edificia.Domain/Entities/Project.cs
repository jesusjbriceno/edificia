using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
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

    /// <summary>ID del usuario que creó el proyecto (propietario original).</summary>
    public Guid CreatedByUserId { get; private set; }

    /// <summary>Navegación al usuario creador.</summary>
    public ApplicationUser CreatedByUser { get; private set; } = null!;

    /// <summary>Miembros del proyecto con sus roles.</summary>
    private readonly List<ProjectMember> _members = [];
    public IReadOnlyCollection<ProjectMember> Members => _members.AsReadOnly();

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

    /// <summary>Adds a member to the project.</summary>
    public void AddMember(Guid userId, ProjectMemberRole role)
    {
        if (_members.Any(m => m.UserId == userId))
            return;

        _members.Add(ProjectMember.Create(Id, userId, role));
    }

    /// <summary>Removes a member from the project.</summary>
    public void RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member is not null)
            _members.Remove(member);
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

    /// <summary>
    /// Updates the content of a specific section within the content tree.
    /// Recursively searches chapters and nested sections by ID.
    /// </summary>
    /// <param name="sectionId">The unique identifier of the section to update.</param>
    /// <param name="content">The new content for the section.</param>
    /// <returns>True if the section was found and updated; false otherwise.</returns>
    public bool UpdateSectionContent(string sectionId, string content)
    {
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
