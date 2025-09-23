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
}
