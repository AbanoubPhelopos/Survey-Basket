using Microsoft.EntityFrameworkCore;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Results;
using Survey_Basket.Application.Errors;
using Survey_Basket.Infrastructure.Data;

namespace Survey_Basket.Application.Services.ResultServices;

public class ResultService : IResultService
{
    private readonly ApplicationDbContext _context;
    public ResultService(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Result<PollVotesResponse>> GetPollVotesAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        var pollVotes = await _context.Polls
            .Where(p => p.Id == pollId)
            .Select(p => new PollVotesResponse
            (
                p.Title,
                p.Votes.Select(v => new VotesResponse(
                    $"{v.User.FirstName} {v.User.LastName}",
                    v.SubmittedOn,
                    v.Answers.Select(a => new QuestionAnswerResponse(
                        a.Question.Content,
                        a.Answer.Content
                        ))

                    ))
            ))
            .SingleOrDefaultAsync(cancellationToken);

        return pollVotes is null
            ? Result.Failure<PollVotesResponse>(PollErrors.PollNotFound)
            : Result.Success(pollVotes);
    }


    public async Task<Result<IEnumerable<VotesPerDayResponse>>> GetPollVotesPerDayAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        var isPollExist = await _context.Polls.AnyAsync(p => p.Id == pollId && p.IsPublished && p.StartedAt <= DateOnly.FromDateTime(DateTime.UtcNow)
                           && p.EndedAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
        if (!isPollExist)
            return Result.Failure<IEnumerable<VotesPerDayResponse>>(PollErrors.PollNotFound);

        var votesPerDay = await _context.Votes
            .Where(x => x.PollId == pollId)
            .GroupBy(x => new { Data = DateOnly.FromDateTime(x.SubmittedOn) })
            .Select(g => new VotesPerDayResponse(
                g.Key.Data,
                g.Count()
                )).ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<VotesPerDayResponse>>(votesPerDay);
    }

}
