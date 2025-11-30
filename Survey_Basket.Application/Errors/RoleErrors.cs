using Microsoft.AspNetCore.Http;
using Survey_Basket.Application.Abstraction;

namespace Survey_Basket.Application.Errors;

public static class RoleErrors
{

    public static readonly Error RoleNotFound = new("Role.NotFound", "Role not found", StatusCodes.Status404NotFound);
}
