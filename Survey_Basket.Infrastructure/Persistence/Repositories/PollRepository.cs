namespace Survey_Basket.Infrastructure.Persistence.Repositories;

public class PollRepository(ApplicationDbContext context) : BaseRepository<Poll>(context), IPollRepository
{
    public async Task<PollVotesResult?> GetPollVotesAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        return await _context.Polls
            .Where(p => p.Id == pollId)
            .Select(p => new PollVotesResult
            (
                p.Title,
                p.Votes.Select(v => new VoteResult(
                    $"{v.User.FirstName} {v.User.LastName}",
                    v.SubmittedOn,
                    v.Answers.Select(a => new QuestionAnswerResult(
                        a.Question.Content,
                        a.Answer.Content
                        ))
                    ))
            ))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
