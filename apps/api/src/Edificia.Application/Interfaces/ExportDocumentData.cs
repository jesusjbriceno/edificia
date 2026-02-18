using Edificia.Domain.Entities;
using Edificia.Domain.Enums;

namespace Edificia.Application.Interfaces;

/// <summary>
/// Data transfer object containing all information needed to generate a document export.
/// </summary>
/// <param name="Title">Project title used for document header and filename.</param>
/// <param name="InterventionType">Localized intervention type (Obra Nueva, Reforma, Ampliación).</param>
/// <param name="IsLoeRequired">Whether the project is subject to LOE regulations.</param>
/// <param name="ContentTreeJson">The full content tree JSON structure with chapters and sections.</param>
/// <param name="Address">Optional project address.</param>
public sealed record ExportDocumentData(
    string Title,
    string InterventionType,
    bool IsLoeRequired,
    string ContentTreeJson,
    string? Address = null)
{
    /// <summary>
    /// Creates an ExportDocumentData from a Project entity, formatting the intervention type.
    /// </summary>
    public static ExportDocumentData FromProject(Project project) => new(
        Title: project.Title,
        InterventionType: FormatInterventionType(project.InterventionType),
        IsLoeRequired: project.IsLoeRequired,
        ContentTreeJson: project.ContentTreeJson!,
        Address: project.Address);

    internal static string FormatInterventionType(Edificia.Domain.Enums.InterventionType interventionType)
        => interventionType switch
    {
        Edificia.Domain.Enums.InterventionType.NewConstruction => "Obra Nueva",
        Edificia.Domain.Enums.InterventionType.Reform => "Reforma",
        Edificia.Domain.Enums.InterventionType.Extension => "Ampliación",
        _ => interventionType.ToString()
    };
}
