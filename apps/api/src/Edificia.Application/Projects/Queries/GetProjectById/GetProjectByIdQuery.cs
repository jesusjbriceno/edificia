using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Queries.GetProjectById;

/// <summary>
/// Query to retrieve a single project by its ID.
/// Uses Dapper (read-side) for optimized reads.
/// </summary>
public sealed record GetProjectByIdQuery(Guid Id) : IRequest<Result<ProjectResponse>>;
