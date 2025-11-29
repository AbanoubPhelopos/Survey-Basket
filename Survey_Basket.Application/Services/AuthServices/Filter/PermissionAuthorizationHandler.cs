using Microsoft.AspNetCore.Authorization;

namespace Survey_Basket.Application.Services.AuthServices.Filter
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User.Identity is not { IsAuthenticated: true } ||
                !context.User.Claims.Any(x => x.Value == requirement.permission))
                return Task.CompletedTask;

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}