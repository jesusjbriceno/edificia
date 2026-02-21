using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Commands.SubmitForReview;

/// <summary>
/// Sends a project for admin review.
/// Transitions status from Draft/InProgress → PendingReview.
/// </summary>
public sealed record SubmitForReviewCommand(Guid ProjectId) : IRequest<Result>;
