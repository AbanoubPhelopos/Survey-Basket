using Hangfire;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Polls;
using Survey_Basket.Application.Errors;
using Survey_Basket.Application.Services.NotificationServices;
using Survey_Basket.Domain.Models;
using Survey_Basket.Infrastructure.Data;

namespace Survey_Basket.Application.Services.PollServices;

public class PollService : IPollService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public PollService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<Result> CreatePollAsync(CreatePollRequests poll)
    {
        var existingPoll = await _context.Polls
            .AnyAsync(p => p.Title == poll.Title);
        if (existingPoll)
            return Result.Failure(PollErrors.PollAlreadyExists);

        try
        {
            var entity = poll.Adapt<Poll>();
            await _context.Polls.AddAsync(entity);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch
        {
            return Result.Failure(PollErrors.PollCreationFailed);
        }
    }

    public async Task<Result> DeletePoll(Guid id)
    {
        var existingPoll = await _context.Polls.SingleOrDefaultAsync(p => p.Id == id);
        if (existingPoll is null)
            return Result.Failure(PollErrors.PollNotFound);

        _context.Polls.Remove(existingPoll);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<PollResponse>> Get(Guid id, CancellationToken cancellationToken = default)
    {
        var poll = await _context.Polls.FindAsync(new object[] { id }, cancellationToken);
        return poll is not null
            ? Result.Success(poll.Adapt<PollResponse>())
            : Result.Failure<PollResponse>(PollErrors.PollNotFound);
    }

    public async Task<Result<IEnumerable<PollResponse>>> Get(CancellationToken cancellationToken = default)
    {
        var polls = await _context.Polls.AsNoTracking().ToListAsync(cancellationToken);
        var response = polls.Adapt<IEnumerable<PollResponse>>();
        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<PollResponse>>> GetCurrent(CancellationToken cancellationToken = default)
    {
        var polls = await _context.Polls
            .Where(p => p.IsPublished && p.StartedAt <= DateOnly.FromDateTime(DateTime.UtcNow)
                            && p.EndedAt >= DateOnly.FromDateTime(DateTime.UtcNow))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var response = polls.Adapt<IEnumerable<PollResponse>>();
        return Result.Success(response);
    }

    public async Task<Result> UpdatePoll(Guid id, UpdatePollRequests updatedPoll)
    {
        var existingPoll = await _context.Polls.SingleOrDefaultAsync(p => p.Id == id);

        if (existingPoll is null)
            return Result.Failure(PollErrors.PollNotFound);

        var pollWithSameTitle = await _context.Polls.AnyAsync(p => p.Title == updatedPoll.Title && p.Id != id);
        if (pollWithSameTitle)
            return Result.Failure(PollErrors.PollAlreadyExists);

        updatedPoll.Adapt(existingPoll);
        _context.Polls.Update(existingPoll);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var poll = await _context.Polls.FindAsync(id, cancellationToken);

        if (poll is null)
            return Result.Failure(PollErrors.PollNotFound);

        poll.IsPublished = !poll.IsPublished;

        await _context.SaveChangesAsync(cancellationToken);

        if (poll.IsPublished && poll.StartedAt == DateOnly.FromDateTime(DateTime.UtcNow))
            BackgroundJob.Enqueue(() => _notificationService.SendNewPollsNotification(poll.Id));

        return Result.Success();
    }
}
