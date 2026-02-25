using Survey_Basket.Application.Abstractions;
using Survey_Basket.Application.Contracts.Common;
using Survey_Basket.Application.Contracts.Roles;

namespace Survey_Basket.Application.Services.RoleService
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleResponse>> GetRoles(bool? includeDisables = false, CancellationToken cancellationToken = default);
        Task<Result<ServiceListResult<RoleResponse, RoleStatsResponse>>> GetRolesFilterResult(RequestFilters filters, string? status, bool? includeDisables = false, CancellationToken cancellationToken = default);
        Task<Result<RoleStatsResponse>> GetRoleStats(CancellationToken cancellationToken = default);
        Task<Result<RoleDetailResponse>> GetRole(string roleId, CancellationToken cancellationToken = default);
        Task<Result<RoleDetailResponse>> CreateRole(RoleRequest request, CancellationToken cancellationToken = default);
        Task<Result> UpdateRole(string roleId, RoleRequest request, CancellationToken cancellationToken = default);

        Task<Result> ToggleRole(string roleId, CancellationToken cancellationToken = default);
    }
}
