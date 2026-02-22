using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Ai.Commands.GenerateSectionText;

/// <summary>
/// Command to generate AI text for a specific section of a project's content tree.
/// Delegates to IAiService (n8n webhook) via Infrastructure layer.
/// </summary>
public sealed record GenerateSectionTextCommand(
    Guid ProjectId,
    string SectionId,
    string Prompt,
    string? Context) : IRequest<Result<GeneratedTextResponse>>
{
    /// <summary>
    /// Maps the DTO fields. <see cref="ProjectId"/> must be enriched
    /// from the route param in the controller using a <c>with</c> expression.
    /// </summary>
    public static explicit operator GenerateSectionTextCommand(GenerateTextRequest r)
        => new(Guid.Empty, r.SectionId, r.Prompt, r.Context);
}
