using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Queries.GetProjectTree;

/// <summary>
/// Query to retrieve the content tree JSON of a project.
/// Returns the raw JSONB column via Dapper (read-side).
/// </summary>
public sealed record GetProjectTreeQuery(Guid ProjectId) : IRequest<Result<ContentTreeResponse>>;
