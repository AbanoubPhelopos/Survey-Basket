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
            .Select(g => new VotesPerDayResult(g.Key.Data, g.Count()))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<VotesPerQuestionResult>> GetVotesPerQuestionAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        var answers = await _context.VoteAnswers
            .Include(x => x.Question)
            .Include(x => x.Answer)
            .Where(x => x.Vote.PollId == pollId)
            .ToListAsync(cancellationToken);

        var grouped = answers
            .GroupBy(x => new { x.QuestionId, QuestionContent = x.Question.Content })
            .Select(questionGroup => new VotesPerQuestionResult(
                questionGroup.Key.QuestionContent,
                questionGroup
                    .GroupBy(FormatVoteAnswer)
                    .Select(answerGroup => new VotesPerAnswerResult(answerGroup.Key, answerGroup.Count()))
            ));

        return grouped;
    }

    private static string FormatVoteAnswer(VoteAnswers answer)
    {
        if (answer.Answer is not null)
            return answer.Answer.Content;

        if (!string.IsNullOrWhiteSpace(answer.SelectedOptionIdsJson))
            return answer.SelectedOptionIdsJson;

        if (answer.BoolValue.HasValue)
            return answer.BoolValue.Value ? "True" : "False";

        if (answer.NumberValue.HasValue)
            return answer.NumberValue.Value.ToString();

        if (!string.IsNullOrWhiteSpace(answer.CountryCode))
            return answer.CountryCode;

        if (!string.IsNullOrWhiteSpace(answer.TextValue))
            return answer.TextValue;

        if (!string.IsNullOrWhiteSpace(answer.FileReference))
            return "[FILE_UPLOADED]";

        return "[EMPTY]";
    }
}
