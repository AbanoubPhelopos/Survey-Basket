namespace Survey_Basket.Infrastructure.Persistence.Repositories;

public class PollRepository(ApplicationDbContext context) : BaseRepository<Poll>(context), IPollRepository
{
    public async Task<PollVotesResult?> GetPollVotesAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        var poll = await _context.Polls
            .Include(p => p.Votes)
                .ThenInclude(v => v.User)
            .Include(p => p.Votes)
                .ThenInclude(v => v.Answers)
                    .ThenInclude(a => a.Question)
            .Include(p => p.Votes)
                .ThenInclude(v => v.Answers)
                    .ThenInclude(a => a.Answer)
            .SingleOrDefaultAsync(p => p.Id == pollId, cancellationToken);

        if (poll is null)
            return null;

        return new PollVotesResult(
            poll.Title,
            poll.Votes.Select(v => new VoteResult(
                $"{v.User.FirstName} {v.User.LastName}",
                v.SubmittedOn,
                v.Answers.Select(a => new QuestionAnswerResult(
                    a.Question.Content,
                    FormatVoteAnswer(a)
                ))
            ))
        );
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
