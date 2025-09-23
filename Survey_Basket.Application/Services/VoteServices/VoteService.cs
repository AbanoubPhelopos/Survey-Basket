using Mapster;
using Microsoft.EntityFrameworkCore;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Votes;
using Survey_Basket.Application.Errors;
using Survey_Basket.Domain.Models;
using Survey_Basket.Infrastructure.Data;

namespace Survey_Basket.Application.Services.VoteServices;

public class VoteService : IVoteService
{
    private readonly ApplicationDbContext _context;
    public VoteService(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Result> AddAsync(Guid pollId, Guid userId, VoteRequest request, CancellationToken cancellationToken = default)
    {
        var hasVote = await _context.Votes.AnyAsync(x => x.PollId == pollId && x.UserId == userId, cancellationToken);

        if (hasVote)
            return Result.Failure(VoteErrors.VoteAlreadyExists);

        var isPollExist = await _context.Polls.AnyAsync(p => p.Id == pollId && p.IsPublished && p.StartedAt <= DateOnly.FromDateTime(DateTime.UtcNow)
                            && p.EndedAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
        if (!isPollExist)
            return Result.Failure(PollErrors.PollNotFound);

        var availableQuestions = await _context.Questions
            .Where(x => x.PollId == pollId && x.IsActive)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (!request.Answers.Select(x => x.QuestionId).SequenceEqual(availableQuestions))
            return Result.Failure(VoteErrors.InvalidQuestionsInVote);

        var vote = new Vote
        {
            PollId = pollId,
            UserId = userId,
            Answers = request.Answers.Adapt<IEnumerable<VoteAnswers>>().ToList()
        };
        await _context.Votes.AddAsync(vote, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();

    }
}
