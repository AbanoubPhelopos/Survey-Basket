using Microsoft.AspNetCore.Authorization;

namespace Survey_Basket.Application.Services.AuthServices.Filter
{
    public class HasPermissionAttribute(string permission) : AuthorizeAttribute(permission)
    {

    }
}