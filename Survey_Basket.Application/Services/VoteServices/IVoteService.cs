using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Votes;

namespace Survey_Basket.Application.Services.VoteServices;

public interface IVoteService
{
    Task<Result> AddAsync(Guid pollId, Guid userId, VoteRequest request, CancellationToken cancellationToken = default!);
}
