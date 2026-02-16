using Edificia.Shared.Result;

namespace Edificia.Application.Users;

/// <summary>
/// Domain errors for user management operations.
/// </summary>
public static class UserErrors
{
    public static readonly Error UserNotFound =
        Error.NotFound("User.NotFound", "El usuario no existe.");

    public static readonly Error EmailAlreadyExists =
        Error.Conflict("User.EmailAlreadyExists", "Ya existe un usuario con ese email.");

    public static readonly Error CreationFailed =
        Error.Failure("User.CreationFailed", "No se pudo crear el usuario.");

    public static readonly Error UpdateFailed =
        Error.Failure("User.UpdateFailed", "No se pudo actualizar el usuario.");

    public static readonly Error CannotModifyHigherRole =
        Error.Forbidden("User.CannotModifyHigherRole",
            "No tiene permisos para gestionar usuarios con ese rol.");

    public static readonly Error CannotDeactivateSelf =
        Error.Forbidden("User.CannotDeactivateSelf",
            "No puede desactivar su propia cuenta.");

    public static readonly Error PasswordResetFailed =
        Error.Failure("User.PasswordResetFailed", "No se pudo restablecer la contraseña.");

    public static readonly Error InvalidRole =
        Error.Validation("User.InvalidRole", "El rol especificado no es válido.");

    public static readonly Error RoleChangeFailed =
        Error.Failure("User.RoleChangeFailed", "No se pudo cambiar el rol del usuario.");
}
