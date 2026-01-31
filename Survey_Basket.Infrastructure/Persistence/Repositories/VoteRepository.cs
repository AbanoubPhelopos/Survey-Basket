namespace Survey_Basket.Infrastructure.Persistence.Repositories;

public class VoteRepository : BaseRepository<Vote>, IVoteRepository
{
    public VoteRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<VotesPerDayResult>> GetVotesPerDayAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        return await _context.Votes
            .Where(x => x.PollId == pollId)
            .GroupBy(x => new { Data = DateOnly.FromDateTime(x.SubmittedOn) })
            .Select(g => new VotesPerDayResult(
                g.Key.Data,
                g.Count()
                )).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<VotesPerQuestionResult>> GetVotesPerQuestionAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        return await _context.VoteAnswers
            .Where(x => x.Vote.PollId == pollId)
            .Select(x => new VotesPerQuestionResult(
                x.Question.Content,
                x.Question.Votes.GroupBy(x => new { AnswerId = x.Answer.Id, AnswerContent = x.Answer.Content })
                .Select(g => new VotesPerAnswerResult(
                    g.Key.AnswerContent,
                    g.Count()
                    ))
                )).ToListAsync(cancellationToken);
    }
}
