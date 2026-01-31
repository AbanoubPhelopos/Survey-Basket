using Survey_Basket.Application.Abstractions;
using Survey_Basket.Application.Contracts.Polls;

namespace Survey_Basket.Application.Services.PollServices;

public interface IPollService
{
    Task<Result<IEnumerable<PollResponse>>> Get(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<PollResponse>>> GetCurrent(CancellationToken cancellationToken = default);
    Task<Result<PollResponse>> Get(Guid id, CancellationToken cancellationToken = default);
    Task<Result> CreatePollAsync(CreatePollRequests poll);
    Task<Result> DeletePoll(Guid id);
    Task<Result> UpdatePoll(Guid id, UpdatePollRequests updatedPoll);
    Task<Result> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default);
}
