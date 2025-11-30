using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Roles;
using Survey_Basket.Application.Errors;
using Survey_Basket.Domain.Models;
using Survey_Basket.Infrastructure.Data;
using SurveyBasket.Abstractions.Consts;

namespace Survey_Basket.Application.Services.RoleService
{
    public class RoleService(RoleManager<ApplicationRole> roleManager, ApplicationDbContext context) : IRoleService
    {
        private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
        private readonly ApplicationDbContext _context = context;

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

        public async Task<Result<RoleDetailResponse>> CreateRole(RoleRequest request, CancellationToken cancellationToken = default)
        {
            var roleExists = await _roleManager.FindByNameAsync(request.Name);
            if (roleExists is not null)
                return Result.Failure<RoleDetailResponse>(RoleErrors.RoleAlreadyExists);

            var allowedPermissions = Permissions.GetAllPermissions();
            if (request.permissions.Except(allowedPermissions).Any())
                return Result.Failure<RoleDetailResponse>(RoleErrors.InvalidPermissions);

            var role = new ApplicationRole
            {
                Name = request.Name,
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                var permissions = request.permissions.Select(x => new IdentityRoleClaim<string>
                {
                    ClaimType = Permissions.Type,
                    ClaimValue = x,
                    RoleId = role.Id.ToString()
                });

                await _context.AddRangeAsync(permissions);
                await _context.SaveChangesAsync();

                var response = new RoleDetailResponse(role.Id, role.Name, role.IsDeleted, request.permissions);
                return Result.Success(response);
            }

            var error = result.Errors.First();
            return Result.Failure<RoleDetailResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));

        }

        public async Task<Result> UpdateRole(string roleId, RoleRequest request, CancellationToken cancellationToken = default)
        {
            var roleExists = await _roleManager.Roles.AnyAsync(x => x.Name == request.Name && x.Id != Guid.Parse(roleId), cancellationToken);
            if (!roleExists)
                return Result.Failure(RoleErrors.DublicatedRoleName);

            if (await _roleManager.FindByIdAsync(roleId) is not { } role)
                return Result.Failure(RoleErrors.RoleNotFound);

            var allowedPermissions = Permissions.GetAllPermissions();
            if (request.permissions.Except(allowedPermissions).Any())
                return Result.Failure(RoleErrors.InvalidPermissions);

            role.Name = request.Name;

            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                var currentPermissions = await _context.RoleClaims.Where(x => x.RoleId == role.Id && x.ClaimType == Permissions.Type).Select(x => x.ClaimValue).ToListAsync(cancellationToken);

                var permissionsToAdd = request.permissions.Except(currentPermissions).Select(x => new IdentityRoleClaim<string>
                {
                    ClaimType = Permissions.Type,
                    ClaimValue = x,
                    RoleId = role.Id.ToString()
                });

                var permissionsToRemove = currentPermissions.Except(request.permissions);

                await _context.RoleClaims.Where(x => x.RoleId == role.Id && permissionsToRemove.Contains(x.ClaimValue))
                .ExecuteDeleteAsync();

                await _context.AddRangeAsync(permissionsToAdd);
                await _context.SaveChangesAsync();

                return Result.Success();
            }

            var error = result.Errors.First();
            return Result.Failure<RoleDetailResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        public async Task<Result> ToggleRole(string roleId, CancellationToken cancellationToken = default)
        {
            if (await _roleManager.FindByIdAsync(roleId) is not { } role)
                return Result.Failure(RoleErrors.RoleNotFound);

            role.IsDeleted = !role.IsDeleted;
            await _roleManager.UpdateAsync(role);
            return Result.Success();
        }
    }
}