using Edificia.Application.Common;
using Edificia.Application.Users.DTOs;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Users.Queries.GetUsers;

/// <summary>
/// Query to retrieve a paginated list of users.
/// Supports optional filtering by role, active status, and search term.
/// Uses Dapper (read-side) for optimized reads.
/// </summary>
public sealed record GetUsersQuery(
    int Page = 1,
    int PageSize = 10,
    string? Role = null,
    bool? IsActive = null,
    string? Search = null) : IRequest<Result<PagedResponse<UserResponse>>>;
