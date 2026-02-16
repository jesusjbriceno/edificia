using Edificia.Application.Auth.Commands.Login;
using Edificia.Application.Auth.DTOs;
using Edificia.Domain.Entities;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Auth.Commands.UpdateProfile;

/// <summary>
/// Handles profile update for the authenticated user.
/// Only allows modifying FullName and CollegiateNumber.
/// </summary>
public sealed class UpdateProfileHandler
    : IRequestHandler<UpdateProfileCommand, Result<UpdateProfileResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UpdateProfileHandler> _logger;

    public UpdateProfileHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<UpdateProfileHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<UpdateProfileResponse>> Handle(
        UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        // 1. Find user by ID (from JWT claim)
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
        {
            _logger.LogWarning("Profile update attempt for non-existent user: {UserId}",
                request.UserId);
            return Result.Failure<UpdateProfileResponse>(AuthErrors.InvalidCredentials);
        }

        // 2. Update allowed fields
        user.FullName = request.FullName;
        user.CollegiateNumber = request.CollegiateNumber;
        user.UpdatedAt = DateTime.UtcNow;

        // 3. Persist changes
        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Profile update failed for user {UserId}: {Errors}",
                request.UserId, errors);
            return Result.Failure<UpdateProfileResponse>(AuthErrors.ProfileUpdateFailed);
        }

        _logger.LogInformation("Profile updated for user {UserId}", request.UserId);

        // 4. Build response
        var response = new UpdateProfileResponse(
            Id: user.Id,
            Email: user.Email!,
            FullName: user.FullName,
            CollegiateNumber: user.CollegiateNumber);

        return Result.Success(response);
    }
}
