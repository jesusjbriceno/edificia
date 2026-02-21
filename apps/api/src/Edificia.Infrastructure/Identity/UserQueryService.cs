using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Edificia.Infrastructure.Identity;

/// <summary>
/// Infrastructure implementation of IUserQueryService using ASP.NET Identity.
/// </summary>
public sealed class UserQueryService : IUserQueryService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserQueryService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<Guid>> GetActiveUserIdsByRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        var users = await _userManager.GetUsersInRoleAsync(role);
        return users.Where(u => u.IsActive).Select(u => u.Id).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<Guid>> GetActiveUserIdsByRolesAsync(IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        var allUserIds = new HashSet<Guid>();

        foreach (var role in roles)
        {
            var users = await _userManager.GetUsersInRoleAsync(role);
            foreach (var user in users.Where(u => u.IsActive))
            {
                allUserIds.Add(user.Id);
            }
        }

        return allUserIds.ToList().AsReadOnly();
    }
}
