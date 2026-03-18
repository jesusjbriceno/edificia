using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Export.Queries.ExportProject;

/// <summary>
/// Query to export a project's content tree as a .docx document.
/// </summary>
/// <param name="ProjectId">The ID of the project to export.</param>
/// <param name="TemplateId">Optional preferred template ID selected by the user.</param>
/// <param name="OutputFileName">Optional output file name requested by the user.</param>
public sealed record ExportProjectQuery(
	Guid ProjectId,
	Guid? TemplateId = null,
	string? OutputFileName = null) : IRequest<Result<ExportDocumentResponse>>;
