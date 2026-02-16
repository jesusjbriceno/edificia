using Edificia.Application.Users.DTOs;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Users.Queries.GetUserById;

/// <summary>
/// Query to retrieve a single user by their ID.
/// Uses Dapper (read-side) for optimized reads.
/// </summary>
public sealed record GetUserByIdQuery(Guid Id) : IRequest<Result<UserResponse>>;
