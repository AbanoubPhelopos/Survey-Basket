using Microsoft.AspNetCore.Authorization;

namespace Survey_Basket.Application.Services.AuthServices.Filter
{
    public class PermissionRequirement(string permission) : IAuthorizationRequirement
    {
        public string permission { get; } = permission;
    }
}