using Edificia.Domain.Constants;
using FluentValidation;

namespace Edificia.Application.Users.Queries.GetUsers;

/// <summary>
/// Validates pagination and filter parameters for GetUsersQuery.
/// </summary>
public sealed class GetUsersValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("La página debe ser mayor o igual a 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("El tamaño de página debe estar entre 1 y 50.");

        RuleFor(x => x.Role)
            .Must(r => AppRoles.All.Contains(r!))
            .WithMessage($"El rol no es válido. Valores permitidos: {string.Join(", ", AppRoles.All)}.")
            .When(x => x.Role is not null);
    }
}
