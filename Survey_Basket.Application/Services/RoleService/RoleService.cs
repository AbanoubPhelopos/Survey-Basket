using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Survey_Basket.Application.Contracts.Roles;
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
    }
}