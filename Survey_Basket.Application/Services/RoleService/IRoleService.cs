using Survey_Basket.Application.Contracts.Roles;

namespace Survey_Basket.Application.Services.RoleService
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleResponse>> GetRoles(bool? includeDisables = false, CancellationToken cancellationToken=default);
    }
}