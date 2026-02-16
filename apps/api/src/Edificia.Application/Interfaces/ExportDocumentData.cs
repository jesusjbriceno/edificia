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
    string? Address = null);
