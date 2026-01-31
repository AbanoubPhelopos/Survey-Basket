using Survey_Basket.Domain.Entities;
using Survey_Basket.Domain.Models;

namespace Survey_Basket.Domain.Abstractions.Repositories;

public interface IPollRepository : IBaseRepository<Poll>
{
    Task<PollVotesResult?> GetPollVotesAsync(Guid pollId, CancellationToken cancellationToken = default);
}
