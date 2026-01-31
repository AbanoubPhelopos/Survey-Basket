using Survey_Basket.Application.Abstractions;
using Survey_Basket.Application.Contracts.Results;

namespace Survey_Basket.Application.Services.ResultServices;

public interface IResultService
{
    Task<Result<PollVotesResponse>> GetPollVotesAsync(Guid pollId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<VotesPerDayResponse>>> GetPollVotesPerDayAsync(Guid pollId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<VotesPerQuestionResponse>>> GetPollVotesPerQuestionAsync(Guid pollId, CancellationToken cancellationToken = default);
}
