using Edificia.Domain.Enums;

namespace Edificia.Application.Projects.Queries;

/// <summary>
/// Response DTO returned by project queries.
/// </summary>
public sealed record ProjectResponse(
    Guid Id,
    string Title,
    string? Description,
    string? Address,
    string InterventionType,
    bool IsLoeRequired,
    string? CadastralReference,
    string? LocalRegulations,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
