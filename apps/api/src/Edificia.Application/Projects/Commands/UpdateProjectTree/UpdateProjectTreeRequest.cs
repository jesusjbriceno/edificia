namespace Edificia.Application.Projects.Commands.UpdateProjectTree;

/// <summary>
/// Request DTO for updating the content tree of a project.
/// </summary>
public sealed record UpdateProjectTreeRequest(string ContentTreeJson);
