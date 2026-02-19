using Survey_Basket.Domain.Entities;

namespace Survey_Basket.Domain.Abstractions.Repositories;

public interface ICompanyRepository : IBaseRepository<Company>
{
    Task<IEnumerable<Company>> GetActiveAsync(CancellationToken cancellationToken = default);
}
