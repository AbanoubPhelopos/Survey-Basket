using Microsoft.AspNetCore.Http;
using Survey_Basket.Application.Abstraction;

namespace Survey_Basket.Application.Errors;

public static class RoleErrors
{

    public static readonly Error RoleNotFound = new("Role.NotFound", "Role not found", StatusCodes.Status404NotFound);
    public static readonly Error RoleAlreadyExists = new("Role.AlreadyExists", "Role already exists", StatusCodes.Status409Conflict);
    public static readonly Error RoleCreationFailed = new("Role.CreationFailed", "Role creation failed", StatusCodes.Status500InternalServerError);
    public static readonly Error RoleDeletionFailed = new("Role.DeletionFailed", "Role deletion failed", StatusCodes.Status500InternalServerError);
    public static readonly Error InvalidPermissions = new("Permission.InvalidPermissions", "Inavlid permission", StatusCodes.Status400BadRequest);
    public static readonly Error DublicatedRoleName = new("Role.DublicatedName", "Role name already exists", StatusCodes.Status409Conflict);
}
