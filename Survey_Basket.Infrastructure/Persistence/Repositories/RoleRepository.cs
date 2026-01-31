namespace Survey_Basket.Infrastructure.Persistence.Repositories;

public class RoleRepository(ApplicationDbContext context) : IRoleRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<string>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.RoleClaims
            .Where(x => x.RoleId == roleId && x.ClaimType == Permissions.Type)
            .Select(x => x.ClaimValue!)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UpdateRolePermissionsAsync(Guid roleId, IEnumerable<string> permissions, CancellationToken cancellationToken = default)
    {
        var currentPermissions = await GetRolePermissionsAsync(roleId, cancellationToken);

        var permissionsToAdd = permissions.Except(currentPermissions)
            .Select(x => new IdentityRoleClaim<Guid>
            {
                ClaimType = Permissions.Type,
                ClaimValue = x,
                RoleId = roleId
            });

        var permissionsToRemove = currentPermissions.Except(permissions);

        if (permissionsToRemove.Any())
        {
            await _context.RoleClaims
                .Where(x => x.RoleId == roleId && permissionsToRemove.Contains(x.ClaimValue))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (permissionsToAdd.Any())
        {
            await _context.AddRangeAsync(permissionsToAdd, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    public async Task<IEnumerable<string>> GetPermissionsByRolesAsync(IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        return await (from r in _context.Roles
                      join p in _context.RoleClaims
                      on r.Id equals p.RoleId
                      where roles.Contains(r.Name!)
                      select p.ClaimValue!)
                      .Distinct()
                      .ToListAsync(cancellationToken);
    }
}
