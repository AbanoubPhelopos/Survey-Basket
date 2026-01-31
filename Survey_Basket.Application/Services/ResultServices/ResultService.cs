using Survey_Basket.Application.Abstractions;
using Survey_Basket.Application.Contracts.Results;
using Survey_Basket.Application.Errors;
using Survey_Basket.Domain.Abstractions;

namespace Survey_Basket.Application.Services.ResultServices;

public class ResultService(IUnitOfWork unitOfWork) : IResultService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<PollVotesResponse>> GetPollVotesAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        var pollVotes = await _unitOfWork.Polls.GetPollVotesAsync(pollId, cancellationToken);

        if (pollVotes is null)
            return Result.Failure<PollVotesResponse>(PollErrors.PollNotFound);

        // Map Domain Result to Contract Response
        var response = new PollVotesResponse(
            pollVotes.Title,
            pollVotes.Votes.Select(v => new VotesResponse(
                v.VoterName,
                v.SubmittedOn,
                v.Answers.Select(a => new QuestionAnswerResponse(
                    a.Question,
                    a.Answer
                ))
            ))
        );

        return Result.Success(response);
    }


    public async Task<Result<IEnumerable<VotesPerDayResponse>>> GetPollVotesPerDayAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        var isPollExist = await _unitOfWork.Polls.AnyAsync(p => p.Id == pollId && p.IsPublished && p.StartedAt <= DateOnly.FromDateTime(DateTime.UtcNow)
                           && p.EndedAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
        if (!isPollExist)
            return Result.Failure<IEnumerable<VotesPerDayResponse>>(PollErrors.PollNotFound);

        var votesPerDay = await _unitOfWork.Votes.GetVotesPerDayAsync(pollId, cancellationToken);

        var response = votesPerDay.Select(x => new VotesPerDayResponse(x.Date, x.Count));

        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<VotesPerQuestionResponse>>> GetPollVotesPerQuestionAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        var isPollExist = await _unitOfWork.Polls.AnyAsync(p => p.Id == pollId && p.IsPublished && p.StartedAt <= DateOnly.FromDateTime(DateTime.UtcNow)
                           && p.EndedAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
        if (!isPollExist)
            return Result.Failure<IEnumerable<VotesPerQuestionResponse>>(PollErrors.PollNotFound);

        var votesPerQuestion = await _unitOfWork.Votes.GetVotesPerQuestionAsync(pollId, cancellationToken);

        var response = votesPerQuestion.Select(x => new VotesPerQuestionResponse(
            x.QuestionContent,
            x.VoteAnswers.Select(a => new VotesPerAnswerResponse(a.AnswerContent, a.Count))
        ));

        return Result.Success(response);
    }

}
