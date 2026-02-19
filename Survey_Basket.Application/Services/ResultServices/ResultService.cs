using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;
using Survey_Basket.Application.Abstractions;
using Survey_Basket.Application.Abstractions.Const;
using Survey_Basket.Application.Contracts.Results;
using Survey_Basket.Application.Errors;
using Survey_Basket.Domain.Abstractions;
using Survey_Basket.Domain.Entities;

namespace Survey_Basket.Application.Services.ResultServices;

public class ResultService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor) : IResultService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<Result<PollVotesResponse>> GetPollVotesAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        var access = await EnsurePollAnalyticsAccessAsync(pollId, cancellationToken);
        if (!access.IsSuccess)
            return Result.Failure<PollVotesResponse>(access.Error);

        var pollVotes = await _unitOfWork.Polls.GetPollVotesAsync(pollId, cancellationToken);

        if (pollVotes is null)
            return Result.Failure<PollVotesResponse>(PollErrors.PollNotFound);

        var response = new PollVotesResponse(
            pollVotes.Title,
            pollVotes.Votes.Select(v => new VotesResponse(
                v.VoterName,
                v.SubmittedOn,
                v.Answers.Select(a => new QuestionAnswerResponse(a.Question, a.Answer))
            ))
        );

        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<VotesPerDayResponse>>> GetPollVotesPerDayAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        var access = await EnsurePollAnalyticsAccessAsync(pollId, cancellationToken);
        if (!access.IsSuccess)
            return Result.Failure<IEnumerable<VotesPerDayResponse>>(access.Error);

        var votesPerDay = await _unitOfWork.Votes.GetVotesPerDayAsync(pollId, cancellationToken);

        var response = votesPerDay.Select(x => new VotesPerDayResponse(x.Date, x.Count));

        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<VotesPerQuestionResponse>>> GetPollVotesPerQuestionAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        var access = await EnsurePollAnalyticsAccessAsync(pollId, cancellationToken);
        if (!access.IsSuccess)
            return Result.Failure<IEnumerable<VotesPerQuestionResponse>>(access.Error);

        var votesPerQuestion = await _unitOfWork.Votes.GetVotesPerQuestionAsync(pollId, cancellationToken);

        var response = votesPerQuestion.Select(x => new VotesPerQuestionResponse(
            x.QuestionContent,
            x.VoteAnswers.Select(a => new VotesPerAnswerResponse(a.AnswerContent, a.Count))
        ));

        return Result.Success(response);
    }

    public async Task<Result<PollAnalyticsResponse>> GetPollAnalyticsAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        var access = await EnsurePollAnalyticsAccessAsync(pollId, cancellationToken);
        if (!access.IsSuccess)
            return Result.Failure<PollAnalyticsResponse>(access.Error);

        var poll = await _unitOfWork.Repository<Poll>()
            .GetAsync(
                x => x.Id == pollId,
                [nameof(Poll.Questions), nameof(Poll.Votes), nameof(Poll.Votes) + "." + nameof(Vote.Answers)],
                cancellationToken);

        if (poll is null)
            return Result.Failure<PollAnalyticsResponse>(PollErrors.PollNotFound);

        var questionBuckets = poll.Questions
            .OrderBy(x => x.DisplayOrder)
            .Select(question =>
            {
                var answers = poll.Votes
                    .SelectMany(v => v.Answers)
                    .Where(a => a.QuestionId == question.Id)
                    .ToList();

                var buckets = answers
                    .Select(FormatVoteAnswer)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .GroupBy(x => x)
                    .Select(g => new AnalyticsBucketResponse(g.Key, g.Count()))
                    .OrderByDescending(x => x.Count)
                    .ThenBy(x => x.Value)
                    .ToList();

                return new QuestionAnalyticsResponse(
                    question.Id,
                    question.Content,
                    question.Type.ToString(),
                    answers.Count,
                    buckets
                );
            })
            .ToList();

        return Result.Success(new PollAnalyticsResponse(
            poll.Id,
            poll.Title,
            poll.Votes.Count,
            questionBuckets
        ));
    }

    private async Task<Result> EnsurePollAnalyticsAccessAsync(Guid pollId, CancellationToken cancellationToken)
    {
        var userContext = GetCurrentUserContext();
        if (!userContext.IsSuccess)
            return Result.Failure(userContext.Error);

        var poll = await _unitOfWork.Repository<Poll>().GetByIdAsync(pollId, cancellationToken);
        if (poll is null)
            return Result.Failure(PollErrors.PollNotFound);

        if (HasAnyRole(userContext.Value.Roles, DefaultRoles.Admin, DefaultRoles.SystemAdmin))
            return Result.Success();

        if (!HasAnyRole(userContext.Value.Roles, DefaultRoles.PartnerCompany))
            return Result.Failure(PollErrors.PollAccessDenied);

        var ownsPoll = await _unitOfWork.Repository<PollOwner>()
            .AnyAsync(x => x.PollId == pollId && x.UserId == userContext.Value.UserId, cancellationToken);

        return ownsPoll ? Result.Success() : Result.Failure(PollErrors.PollAccessDenied);
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

    private Result<(Guid UserId, List<string> Roles)> GetCurrentUserContext()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity is not { IsAuthenticated: true })
            return Result.Failure<(Guid, List<string>)>(PollErrors.PollAccessDenied);

        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Result.Failure<(Guid, List<string>)>(PollErrors.PollAccessDenied);

        var rolesClaim = user.Claims.FirstOrDefault(x => x.Type == "roles")?.Value;
        var roles = DeserializeRoles(rolesClaim);

        return Result.Success((userId, roles));
    }

    private static List<string> DeserializeRoles(string? rolesClaim)
    {
        if (string.IsNullOrWhiteSpace(rolesClaim))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<string>>(rolesClaim) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static bool HasAnyRole(IEnumerable<string> roles, params string[] expected)
    {
        var set = roles.Select(x => x.ToLowerInvariant()).ToHashSet();
        return expected.Any(x => set.Contains(x.ToLowerInvariant()));
    }
}
