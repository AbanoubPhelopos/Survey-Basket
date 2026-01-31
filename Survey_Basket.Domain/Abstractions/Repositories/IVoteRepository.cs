using Survey_Basket.Domain.Entities;

namespace Survey_Basket.Domain.Abstractions.Repositories;

public interface IVoteRepository : IBaseRepository<Vote>
{
    Task<IEnumerable<VotesPerDayResult>> GetVotesPerDayAsync(Guid pollId, CancellationToken cancellationToken = default);
    Task<IEnumerable<VotesPerQuestionResult>> GetVotesPerQuestionAsync(Guid pollId, CancellationToken cancellationToken = default);
}
