using System.Text;
using Edificia.Domain.Enums;

namespace Edificia.Application.Ai.Services;

/// <summary>
/// Builds contextualized AI prompts by injecting project metadata into structured templates.
/// Adapts prompt content based on intervention type (Obra Nueva / Reforma / Ampliación)
/// and LOE applicability.
/// </summary>
public sealed class PromptTemplateService : IPromptTemplateService
{
    /// <inheritdoc />
    public string BuildSectionPrompt(SectionPromptContext context)
    {
        var sb = new StringBuilder();

        // ── Project Context ──
        sb.AppendLine("## Contexto del Proyecto");
        sb.AppendLine($"- Proyecto: {context.ProjectTitle}");
        sb.AppendLine($"- Tipo de intervención: {FormatInterventionType(context.InterventionType)}");
        sb.AppendLine($"- Normativa LOE: {FormatLoeStatus(context.IsLoeRequired)}");

        if (!string.IsNullOrWhiteSpace(context.Address))
            sb.AppendLine($"- Dirección: {context.Address}");

        if (!string.IsNullOrWhiteSpace(context.LocalRegulations))
            sb.AppendLine($"- Normativa local: {context.LocalRegulations}");

        sb.AppendLine();

        // ── Section Reference ──
        sb.AppendLine("## Sección a Redactar");
        sb.AppendLine($"ID de sección: {context.SectionId}");
        sb.AppendLine();

        // ── Existing Content (optional) ──
        if (!string.IsNullOrWhiteSpace(context.ExistingContent))
        {
            sb.AppendLine("## Contenido existente");
            sb.AppendLine(context.ExistingContent);
            sb.AppendLine();
        }

        // ── User Instruction ──
        sb.AppendLine("## Instrucción");
        sb.AppendLine(context.UserPrompt);

        return sb.ToString();
    }

    private static string FormatInterventionType(InterventionType type) => type switch
    {
        InterventionType.NewConstruction => "Obra Nueva",
        InterventionType.Reform => "Reforma",
        InterventionType.Extension => "Ampliación",
        _ => type.ToString()
    };

    private static string FormatLoeStatus(bool isLoeRequired) =>
        isLoeRequired
            ? "Aplicable (Art. 4 LOE) — normativa completa aplicable"
            : "Proyecto exento (Art. 2.2 LOE)";
}
