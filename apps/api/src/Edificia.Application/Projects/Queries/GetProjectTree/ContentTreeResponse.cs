namespace Edificia.Application.Projects.Queries.GetProjectTree;

/// <summary>
/// Response DTO containing the content tree JSON and project metadata
/// needed for client-side filtering.
/// </summary>
public sealed record ContentTreeResponse(
    Guid ProjectId,
    string InterventionType,
    bool IsLoeRequired,
    string? ContentTreeJson);
