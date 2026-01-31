using Survey_Basket.Domain.Entities;

namespace Survey_Basket.Domain.Abstractions.Repositories;

public interface IUserRepository : IBaseRepository<ApplicationUser>
{
    Task<IEnumerable<UserWithRolesResult>> GetUsersWithRolesAsync(CancellationToken cancellationToken = default);
}
