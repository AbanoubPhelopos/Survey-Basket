using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using Survey_Basket.Application.Abstractions.Const;

namespace Survey_Basket.Application.Services.AuthServices.Filter
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User.Identity is not { IsAuthenticated: true })
                return Task.CompletedTask;

            var permissionsClaim = context.User.Claims.FirstOrDefault(x => x.Type == nameof(Permissions).ToLowerInvariant())?.Value
                                   ?? context.User.Claims.FirstOrDefault(x => x.Type == "permissions")?.Value;

            if (string.IsNullOrWhiteSpace(permissionsClaim) && context.User.Claims.Any(x => x.Value == requirement.permission))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            if (!string.IsNullOrWhiteSpace(permissionsClaim))
            {
                try
                {
                    var permissions = JsonSerializer.Deserialize<List<string>>(permissionsClaim) ?? [];
                    if (permissions.Contains(requirement.permission, StringComparer.OrdinalIgnoreCase))
                    {
                        context.Succeed(requirement);
                        return Task.CompletedTask;
                    }
                }
                catch
                {
                    if (context.User.Claims.Any(x => x.Value == requirement.permission))
                    {
                        context.Succeed(requirement);
                        return Task.CompletedTask;
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
