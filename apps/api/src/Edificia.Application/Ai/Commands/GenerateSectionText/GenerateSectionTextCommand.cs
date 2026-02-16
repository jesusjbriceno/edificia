using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Ai.Commands.GenerateSectionText;

/// <summary>
/// Command to generate AI text for a specific section of a project's content tree.
/// Uses FluxAiService (Infrastructure) via IAiService interface.
/// </summary>
public sealed record GenerateSectionTextCommand(
    Guid ProjectId,
    string SectionId,
    string Prompt,
    string? Context) : IRequest<Result<GeneratedTextResponse>>
{
    public static GenerateSectionTextCommand Create(Guid projectId, GenerateTextRequest r)
        => new(projectId, r.SectionId, r.Prompt, r.Context);
}
