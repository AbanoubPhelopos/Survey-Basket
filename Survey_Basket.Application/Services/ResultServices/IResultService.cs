using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Results;

namespace Survey_Basket.Application.Services.ResultServices;

public interface IResultService
{
    Task<Result<PollVotesResponse>> GetPollVotesAsync(Guid pollId, CancellationToken cancellationToken = default);
}
