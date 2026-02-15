namespace Edificia.Application.Projects.Commands.PatchSectionContent;

/// <summary>
/// Request DTO for updating the content of a specific section.
/// The SectionId comes from the route parameter.
/// </summary>
public sealed record UpdateSectionRequest(string Content);
