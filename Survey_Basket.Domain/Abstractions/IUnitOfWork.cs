using Survey_Basket.Domain.Abstractions.Repositories;

namespace Survey_Basket.Domain.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    IBaseRepository<TEntity> Repository<TEntity>() where TEntity : class;

    IPollRepository Polls { get; }
    IVoteRepository Votes { get; }
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    ICompanyRepository Companies { get; }
    IPollAudienceRepository PollAudiences { get; }
}
