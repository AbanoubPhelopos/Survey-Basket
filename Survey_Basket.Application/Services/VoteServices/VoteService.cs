using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;
using Survey_Basket.Application.Abstractions;
using Survey_Basket.Application.Abstractions.Const;
using Survey_Basket.Application.Contracts.Votes;
using Survey_Basket.Application.Errors;
using Survey_Basket.Domain.Abstractions;
using Survey_Basket.Domain.Entities;

namespace Survey_Basket.Application.Services.VoteServices;

public class VoteService(
    IUnitOfWork unitOfWork,
    IFileAnswerStorage fileAnswerStorage,
    IHttpContextAccessor httpContextAccessor) : IVoteService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IFileAnswerStorage _fileAnswerStorage = fileAnswerStorage;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<Result> AddAsync(Guid pollId, Guid userId, VoteRequest request, CancellationToken cancellationToken = default)
    {
        var roles = GetCurrentRoles();

        var hasVote = await _unitOfWork.Repository<Vote>()
            .AnyAsync(x => x.PollId == pollId && x.UserId == userId, cancellationToken);
        if (hasVote)
            return Result.Failure(VoteErrors.VoteAlreadyExists);

        var poll = await _unitOfWork.Repository<Poll>()
            .GetByIdAsync(pollId, cancellationToken);
        if (poll is null)
            return Result.Failure(PollErrors.PollNotFound);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (!poll.IsPublished || poll.StartedAt > today || (poll.EndedAt.HasValue && poll.EndedAt.Value < today))
            return Result.Failure(PollErrors.PollNotFound);

        var eligibility = await CanUserAnswerPollAsync(userId, pollId, roles, cancellationToken);
        if (!eligibility)
            return Result.Failure(VoteErrors.VoteAccessDenied);

        var availableQuestions = (await _unitOfWork.Repository<Question>()
            .GetAllAsync(x => x.PollId == pollId && x.IsActive, new[] { nameof(Question.Answers) }, cancellationToken))
            .OrderBy(x => x.DisplayOrder)
            .ToList();

        var availableQuestionIds = availableQuestions.Select(x => x.Id).ToHashSet();
        var requestQuestionIds = request.Answers.Select(x => x.QuestionId).ToList();

        if (requestQuestionIds.Count != availableQuestionIds.Count || requestQuestionIds.Distinct().Count() != requestQuestionIds.Count)
            return Result.Failure(VoteErrors.InvalidQuestionsInVote);

        if (!requestQuestionIds.All(availableQuestionIds.Contains))
            return Result.Failure(VoteErrors.InvalidQuestionsInVote);

        var vote = new Vote
        {
            PollId = pollId,
            UserId = userId,
            SubmittedOn = DateTime.UtcNow,
            Answers = []
        };

        foreach (var answerRequest in request.Answers)
        {
            var question = availableQuestions.First(x => x.Id == answerRequest.QuestionId);
            var mapped = await MapAnswerAsync(pollId, userId, question, answerRequest, cancellationToken);
            if (!mapped.IsSuccess)
                return Result.Failure(mapped.Error);

            vote.Answers.Add(mapped.Value);
        }

        await _unitOfWork.Repository<Vote>().AddAsync(vote, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Result<VoteAnswers>> MapAnswerAsync(Guid pollId, Guid userId, Question question, VoteAnswerRequest request, CancellationToken cancellationToken)
    {
        var answer = new VoteAnswers
        {
            QuestionId = question.Id
        };

        switch (question.Type)
        {
            case QuestionType.SingleChoice:
            case QuestionType.Country:
                if (!request.AnswerId.HasValue)
                    return Result.Failure<VoteAnswers>(VoteErrors.InvalidAnswerPayload);

                if (!question.Answers.Any(x => x.Id == request.AnswerId.Value && x.IsActive))
                    return Result.Failure<VoteAnswers>(VoteErrors.InvalidAnswerPayload);

                answer.AnswerId = request.AnswerId.Value;
                break;

            case QuestionType.MultipleChoice:
                var ids = request.AnswerIds?.Distinct().ToList() ?? [];
                if (ids.Count == 0)
                    return Result.Failure<VoteAnswers>(VoteErrors.InvalidAnswerPayload);

                var validIds = question.Answers.Where(x => x.IsActive).Select(x => x.Id).ToHashSet();
                if (ids.Any(x => !validIds.Contains(x)))
                    return Result.Failure<VoteAnswers>(VoteErrors.InvalidAnswerPayload);

                answer.SelectedOptionIdsJson = JsonSerializer.Serialize(ids);
                break;

            case QuestionType.TrueFalse:
                if (request.BoolValue is null)
                    return Result.Failure<VoteAnswers>(VoteErrors.InvalidAnswerPayload);
                answer.BoolValue = request.BoolValue.Value;
                break;

            case QuestionType.Number:
                if (request.NumberValue is null)
                    return Result.Failure<VoteAnswers>(VoteErrors.InvalidAnswerPayload);
                answer.NumberValue = request.NumberValue.Value;
                break;

            case QuestionType.Text:
                if (string.IsNullOrWhiteSpace(request.TextValue))
                    return Result.Failure<VoteAnswers>(VoteErrors.InvalidAnswerPayload);
                answer.TextValue = request.TextValue.Trim();
                break;

            case QuestionType.FileUpload:
                if (request.File is null || string.IsNullOrWhiteSpace(request.File.Base64Content))
                    return Result.Failure<VoteAnswers>(VoteErrors.InvalidAnswerPayload);

                byte[] content;
                try
                {
                    content = Convert.FromBase64String(request.File.Base64Content);
                }
                catch
                {
                    return Result.Failure<VoteAnswers>(VoteErrors.InvalidAnswerPayload);
                }

                var fileRef = await _fileAnswerStorage.SaveAsync(
                    pollId,
                    question.Id,
                    userId,
                    request.File.FileName,
                    request.File.ContentType,
                    content,
                    cancellationToken);

                answer.FileReference = fileRef;
                break;

            default:
                return Result.Failure<VoteAnswers>(VoteErrors.InvalidAnswerPayload);
        }

        return Result.Success(answer);
    }

    private async Task<bool> CanUserAnswerPollAsync(Guid userId, Guid pollId, List<string> roles, CancellationToken cancellationToken)
    {
        if (HasAnyRole(roles, DefaultRoles.Admin, DefaultRoles.SystemAdmin))
            return true;

        var audience = await _unitOfWork.Repository<PollAudience>()
            .GetAllAsync(x => x.PollId == pollId, cancellationToken);

        if (!audience.Any())
            return false;

        if (HasAnyRole(roles, DefaultRoles.PartnerCompany))
        {
            var ownsPoll = await _unitOfWork.Repository<PollOwner>()
                .AnyAsync(x => x.PollId == pollId && x.UserId == userId, cancellationToken);
            if (ownsPoll)
                return true;
        }

        var companyIds = (await _unitOfWork.Repository<CompanyUser>()
            .GetAllAsync(x => x.UserId == userId && x.IsActive, cancellationToken))
            .Select(x => x.CompanyId)
            .ToHashSet();

        return audience.Any(x => companyIds.Contains(x.CompanyId));
    }

    private List<string> GetCurrentRoles()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null)
            return [];

        var rolesClaim = user.Claims.FirstOrDefault(x => x.Type == "roles")?.Value;
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
