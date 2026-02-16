using Edificia.Application.Common;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Queries.GetProjects;

/// <summary>
/// Query to retrieve a paginated list of projects.
/// Supports optional filtering by status and search term.
/// Uses Dapper (read-side) for optimized reads.
/// </summary>
public sealed record GetProjectsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Status = null,
    string? Search = null) : IRequest<Result<PagedResponse<ProjectResponse>>>;
