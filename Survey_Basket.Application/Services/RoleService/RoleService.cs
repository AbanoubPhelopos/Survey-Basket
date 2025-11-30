using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Roles;
using Survey_Basket.Application.Errors;
using Survey_Basket.Domain.Models;
using Survey_Basket.Infrastructure.Data;

namespace Survey_Basket.Application.Services.RoleService
{
    public class RoleService(RoleManager<ApplicationRole> roleManager) : IRoleService
    {
        private readonly RoleManager<ApplicationRole> _roleManager = roleManager;

        public async Task<IEnumerable<RoleResponse>> GetRoles(bool? includeDisables = false, CancellationToken cancellationToken = default)
        => await _roleManager.Roles.Where(x => !x.IsDefault && (!x.IsDeleted || (includeDisables.HasValue && includeDisables.Value)))
        .ProjectToType<RoleResponse>()
        .ToListAsync(cancellationToken);

        public async Task<Result<RoleDetailResponse>> GetRole(string roleId, CancellationToken cancellationToken = default)
        {
            if (await _roleManager.FindByIdAsync(roleId) is not { } role)
                return Result.Failure<RoleDetailResponse>(RoleErrors.RoleNotFound);

            var permissions = await _roleManager.GetClaimsAsync(role);
            var RoleResponse = new RoleDetailResponse(role.Id, role.Name!, role.IsDeleted, permissions.Select(x => x.Value));
            return Result.Success(RoleResponse);
        }
    }
}