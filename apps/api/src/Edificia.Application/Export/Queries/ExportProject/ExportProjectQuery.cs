using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Export.Queries.ExportProject;

/// <summary>
/// Query to export a project's content tree as a .docx document.
/// </summary>
/// <param name="ProjectId">The ID of the project to export.</param>
public sealed record ExportProjectQuery(Guid ProjectId) : IRequest<Result<ExportDocumentResponse>>;
