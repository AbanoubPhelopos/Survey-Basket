using Survey_Basket.Application.Abstractions;
using Survey_Basket.Application.Contracts.Common;
using Survey_Basket.Application.Contracts.Polls;

namespace Survey_Basket.Application.Services.PollServices;

public interface IPollService
{
    Task<Result<PagedList<PollResponse>>> Get(RequestFilters filters, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<PollResponse>>> GetCurrent(CancellationToken cancellationToken = default);
    Task<Result<PollResponse>> Get(Guid id, CancellationToken cancellationToken = default);
    Task<Result> CreatePollAsync(CreatePollRequests poll, CancellationToken cancellationToken = default);
    Task<Result> DeletePoll(Guid id, CancellationToken cancellationToken = default);
    Task<Result> UpdatePoll(Guid id, UpdatePollRequests updatedPoll, CancellationToken cancellationToken = default);
    Task<Result> AssignAudienceAsync(Guid pollId, AssignPollAudienceRequest request, CancellationToken cancellationToken = default);
    Task<Result> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default);
}
