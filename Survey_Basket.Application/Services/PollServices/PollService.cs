using Hangfire;
using Mapster;
using Survey_Basket.Application.Abstractions;
using Survey_Basket.Application.Contracts.Polls;
using Survey_Basket.Application.Errors;
using Survey_Basket.Application.Services.NotificationServices;
using Survey_Basket.Domain.Abstractions;
using Survey_Basket.Domain.Entities;

using Survey_Basket.Application.Contracts.Common;

namespace Survey_Basket.Application.Services.PollServices;

public class PollService(IUnitOfWork unitOfWork, INotificationService notificationService) : IPollService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly INotificationService _notificationService = notificationService;

    public async Task<Result> CreatePollAsync(CreatePollRequests poll)
    {
        var existingPoll = await _unitOfWork.Repository<Poll>()
            .AnyAsync(p => p.Title == poll.Title);
        if (existingPoll)
            return Result.Failure(PollErrors.PollAlreadyExists);

        try
        {
            var entity = new Poll
            {
                Title = poll.Title,
                Summary = poll.Summary,
                StartedAt = poll.StartedAt,
                EndedAt = poll.EndedAt,
                IsPublished = false // Default
            };
            
            await _unitOfWork.Repository<Poll>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Poll.Exception", ex.ToString(), 500));
        }
    }

    public async Task<Result> DeletePoll(Guid id)
    {
        var existingPoll = await _unitOfWork.Repository<Poll>().GetByIdAsync(id);
        if (existingPoll is null)
            return Result.Failure(PollErrors.PollNotFound);

        _unitOfWork.Repository<Poll>().Remove(existingPoll);
        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<PollResponse>> Get(Guid id, CancellationToken cancellationToken = default)
    {
        var poll = await _unitOfWork.Repository<Poll>().GetByIdAsync(id, cancellationToken);
        return poll is not null
            ? Result.Success(poll.Adapt<PollResponse>())
            : Result.Failure<PollResponse>(PollErrors.PollNotFound);
    }

    public async Task<Result<PagedList<PollResponse>>> Get(RequestFilters filters, CancellationToken cancellationToken = default)
    {
        var (polls, totalCount) = await _unitOfWork.Repository<Poll>().GetPagedAsync(
            filters.PageNumber,
            filters.PageSize,
            filters.SortColumn,
            filters.SortDirection,
            p => string.IsNullOrEmpty(filters.SearchTerm) || p.Title.Contains(filters.SearchTerm),
            cancellationToken);

        var pollResponses = polls.Adapt<IEnumerable<PollResponse>>();
        var result = new PagedList<PollResponse>(pollResponses, filters.PageNumber, totalCount, filters.PageSize);

        return Result.Success(result);
    }

    public async Task<Result<IEnumerable<PollResponse>>> GetCurrent(CancellationToken cancellationToken = default)
    {
        var polls = await _unitOfWork.Repository<Poll>()
            .GetAllAsync(p => p.IsPublished && p.StartedAt <= DateOnly.FromDateTime(DateTime.UtcNow)
                            && p.EndedAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);

        var response = polls.Adapt<IEnumerable<PollResponse>>();
        return Result.Success(response);
    }

    public async Task<Result> UpdatePoll(Guid id, UpdatePollRequests updatedPoll)
    {
        var existingPoll = await _unitOfWork.Repository<Poll>().GetByIdAsync(id);

        if (existingPoll is null)
            return Result.Failure(PollErrors.PollNotFound);

        var pollWithSameTitle = await _unitOfWork.Repository<Poll>().AnyAsync(p => p.Title == updatedPoll.Title && p.Id != id);
        if (pollWithSameTitle)
            return Result.Failure(PollErrors.PollAlreadyExists);

        updatedPoll.Adapt(existingPoll);
        _unitOfWork.Repository<Poll>().Update(existingPoll);
        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var poll = await _unitOfWork.Repository<Poll>().GetByIdAsync(id, cancellationToken);

        if (poll is null)
            return Result.Failure(PollErrors.PollNotFound);

        poll.IsPublished = !poll.IsPublished;

        // Explicit update is not strictly required if tracking is on, but good practice with UoW pattern usually.
        // However, BaseRepository.Update calls _dbSet.Update() which attaches as Modified.
        // Since GetByIdAsync returns tracked entity, we just need SaveChanges.
        // But to be consistent with Repository pattern:
        _unitOfWork.Repository<Poll>().Update(poll);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (poll.IsPublished && poll.StartedAt == DateOnly.FromDateTime(DateTime.UtcNow))
            BackgroundJob.Enqueue(() => _notificationService.SendNewPollsNotification(poll.Id));

        return Result.Success();
    }
}
