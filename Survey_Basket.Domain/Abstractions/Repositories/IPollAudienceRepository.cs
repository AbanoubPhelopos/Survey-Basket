using Survey_Basket.Domain.Entities;

namespace Survey_Basket.Domain.Abstractions.Repositories;

public interface IPollAudienceRepository : IBaseRepository<PollAudience>
{
    Task<IEnumerable<PollAudience>> GetByPollIdAsync(Guid pollId, CancellationToken cancellationToken = default);
}
