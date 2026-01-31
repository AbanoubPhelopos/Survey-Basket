namespace Survey_Basket.Domain.Abstractions.Repositories;

public interface IRoleRepository
{
    // Managing Permissions (Claims)
    Task<IEnumerable<string>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<bool> UpdateRolePermissionsAsync(Guid roleId, IEnumerable<string> permissions, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetPermissionsByRolesAsync(IEnumerable<string> roles, CancellationToken cancellationToken = default);
}
