using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;
using Survey_Basket.Application.Abstractions;
using Survey_Basket.Application.Abstractions.Const;
using Survey_Basket.Application.Contracts.Answer;
using Survey_Basket.Application.Contracts.Question;
using Survey_Basket.Application.Errors;
using Survey_Basket.Domain.Abstractions;
using Survey_Basket.Domain.Entities;

namespace Survey_Basket.Application.Services.QuestionServices;

public class QuestionService(
    IUnitOfWork unitOfWork,
    HybridCache hybridCache,
    ILogger<QuestionService> logger,
    IHttpContextAccessor httpContextAccessor) : IQuestionService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly HybridCache _hybridCache = hybridCache;
    private readonly ILogger _logger = logger;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private const string CacheKeyPrefix = "Poll_Questions_";

    public async Task<Result<IEnumerable<QuestionResponse>>> GetQuestionsAsync(Guid pollId, CancellationToken cancellationToken)
    {
        var isPollExist = await _unitOfWork.Repository<Poll>().AnyAsync(p => p.Id == pollId, cancellationToken);
        if (!isPollExist)
            return Result.Failure<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);

        var questions = await _unitOfWork.Repository<Question>()
            .GetAllAsync(x => x.PollId == pollId, new[] { nameof(Question.Answers) }, cancellationToken);

        var response = questions.OrderBy(x => x.DisplayOrder).Adapt<IEnumerable<QuestionResponse>>();

        return Result.Success(response);
    }

    public async Task<Result<QuestionResponse>> GetQuestionAsync(Guid pollId, Guid id, CancellationToken cancellationToken)
    {
        var question = await _unitOfWork.Repository<Question>()
            .GetAsync(q => q.PollId == pollId && q.Id == id, new[] { nameof(Question.Answers) }, cancellationToken);

        if (question is null)
            return Result.Failure<QuestionResponse>(QuestionErrors.QuestionNotFound);

        return Result.Success(question.Adapt<QuestionResponse>());
    }

    public async Task<Result<QuestionResponse>> CreateQuestionAsync(Guid pollId, QuestionRequest request, CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUserContext();
        if (!currentUser.IsSuccess)
            return Result.Failure<QuestionResponse>(currentUser.Error);

        var poll = await _unitOfWork.Repository<Poll>().GetByIdAsync(pollId, cancellationToken);
        if (poll is null)
            return Result.Failure<QuestionResponse>(PollErrors.PollNotFound);

        var canManage = await CanManagePollAsync(pollId, currentUser.Value.UserId, currentUser.Value.Roles, cancellationToken);
        if (!canManage.IsSuccess)
            return Result.Failure<QuestionResponse>(canManage.Error);

        var isQuestionExist = await _unitOfWork.Repository<Question>()
            .AnyAsync(x => x.Content == request.Content && x.PollId == pollId, cancellationToken);
        if (isQuestionExist)
            return Result.Failure<QuestionResponse>(QuestionErrors.QuestionAlreadyExists);

        var normalizedOptions = BuildOptionsForType(request.Type, request.Answers);
        if (normalizedOptions is null)
            return Result.Failure<QuestionResponse>(QuestionErrors.InvalidQuestionOptions);

        var question = new Question
        {
            PollId = pollId,
            Content = request.Content,
            Type = request.Type,
            IsRequired = request.IsRequired,
            DisplayOrder = request.DisplayOrder,
            CreatedById = currentUser.Value.UserId,
            Answers = normalizedOptions.Select(x => new Answer { Content = x, IsActive = true }).ToList()
        };

        await _unitOfWork.Repository<Question>().AddAsync(question, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _hybridCache.RemoveAsync($"{CacheKeyPrefix}{pollId}", cancellationToken);

        return Result.Success(question.Adapt<QuestionResponse>());
    }

    public async Task<Result> UpdateAsync(Guid pollId, Guid id, QuestionRequest request, CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUserContext();
        if (!currentUser.IsSuccess)
            return Result.Failure(currentUser.Error);

        var canManage = await CanManagePollAsync(pollId, currentUser.Value.UserId, currentUser.Value.Roles, cancellationToken);
        if (!canManage.IsSuccess)
            return Result.Failure(canManage.Error);

        var isQuestionExist = await _unitOfWork.Repository<Question>()
            .AnyAsync(x => x.Content == request.Content && x.PollId == pollId && x.Id != id, cancellationToken);
        if (isQuestionExist)
            return Result.Failure(QuestionErrors.QuestionAlreadyExists);

        var question = await _unitOfWork.Repository<Question>()
            .GetAsync(x => x.Id == id && x.PollId == pollId, new[] { nameof(Question.Answers) }, cancellationToken);

        if (question is null)
            return Result.Failure(QuestionErrors.QuestionNotFound);

        var normalizedOptions = BuildOptionsForType(request.Type, request.Answers);
        if (normalizedOptions is null)
            return Result.Failure(QuestionErrors.InvalidQuestionOptions);

        question.Content = request.Content;
        question.Type = request.Type;
        question.IsRequired = request.IsRequired;
        question.DisplayOrder = request.DisplayOrder;
        question.UpdatedById = currentUser.Value.UserId;

        var currentAnswers = question.Answers.Select(x => x.Content).ToList();
        var newAnswers = normalizedOptions.Except(currentAnswers).ToList();

        newAnswers.ForEach(answer => question.Answers.Add(new Answer { Content = answer, IsActive = true }));

        question.Answers.ToList().ForEach(answer =>
        {
            answer.IsActive = normalizedOptions.Contains(answer.Content);
        });

        _unitOfWork.Repository<Question>().Update(question);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _hybridCache.RemoveAsync($"{CacheKeyPrefix}{pollId}", cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ToggleStatusAsync(Guid pollId, Guid id, CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUserContext();
        if (!currentUser.IsSuccess)
            return Result.Failure(currentUser.Error);

        var canManage = await CanManagePollAsync(pollId, currentUser.Value.UserId, currentUser.Value.Roles, cancellationToken);
        if (!canManage.IsSuccess)
            return Result.Failure(canManage.Error);

        var question = await _unitOfWork.Repository<Question>()
            .GetAsync(x => x.Id == id && x.PollId == pollId, cancellationToken);

        if (question is null)
            return Result.Failure(QuestionErrors.QuestionNotFound);

        question.IsActive = !question.IsActive;
        question.UpdatedById = currentUser.Value.UserId;

        _unitOfWork.Repository<Question>().Update(question);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _hybridCache.RemoveAsync($"{CacheKeyPrefix}{pollId}", cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<QuestionResponse>>> GetAvailableQuestionsAsync(Guid pollId, Guid userId, CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUserContext();
        if (!currentUser.IsSuccess)
            return Result.Failure<IEnumerable<QuestionResponse>>(currentUser.Error);

        var hasVote = await _unitOfWork.Repository<Vote>().AnyAsync(x => x.PollId == pollId && x.UserId == userId, cancellationToken);

        if (hasVote)
            return Result.Failure<IEnumerable<QuestionResponse>>(VoteErrors.VoteAlreadyExists);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var isPollExist = await _unitOfWork.Repository<Poll>().AnyAsync(
            p => p.Id == pollId && p.IsPublished && p.StartedAt <= today && (!p.EndedAt.HasValue || p.EndedAt >= today),
            cancellationToken);

        if (!isPollExist)
            return Result.Failure<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);

        if (!HasAnyRole(currentUser.Value.Roles, DefaultRoles.Admin, DefaultRoles.SystemAdmin))
        {
            var companyIds = (await _unitOfWork.Repository<CompanyUser>()
                .GetAllAsync(x => x.UserId == userId && x.IsActive, cancellationToken))
                .Select(x => x.CompanyId)
                .ToHashSet();

            var isTargeted = await _unitOfWork.Repository<PollAudience>()
                .AnyAsync(x => x.PollId == pollId && companyIds.Contains(x.CompanyId), cancellationToken);

            if (!isTargeted)
                return Result.Failure<IEnumerable<QuestionResponse>>(VoteErrors.VoteAccessDenied);
        }

        var cacheKey = $"{CacheKeyPrefix}{pollId}";

        var questions = await _hybridCache.GetOrCreateAsync<IEnumerable<QuestionResponse>>(
            cacheKey,
            async _ =>
            {
                var entities = await _unitOfWork.Repository<Question>()
                    .GetAllAsync(q => q.PollId == pollId && q.IsActive, new[] { nameof(Question.Answers) }, cancellationToken);

                return entities
                    .OrderBy(q => q.DisplayOrder)
                    .Select(q => new QuestionResponse(
                        q.Id,
                        q.Content,
                        q.Type,
                        q.IsRequired,
                        q.DisplayOrder,
                        q.Answers.Where(a => a.IsActive).Select(a => new AnswerResponse(a.Id, a.Content))
                    ));
            });

        return Result.Success(questions);
    }

    private static List<string>? BuildOptionsForType(QuestionType type, List<string> answers)
    {
        var sanitized = answers
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return type switch
        {
            QuestionType.SingleChoice or QuestionType.MultipleChoice when sanitized.Count >= 2 => sanitized,
            QuestionType.TrueFalse when sanitized.Count == 0 => ["True", "False"],
            QuestionType.TrueFalse when sanitized.Count == 2 => sanitized,
            QuestionType.Country when sanitized.Count >= 1 => sanitized,
            QuestionType.Number or QuestionType.Text or QuestionType.FileUpload when sanitized.Count == 0 => [],
            _ => null
        };
    }

    private Result<(Guid UserId, List<string> Roles)> GetCurrentUserContext()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity is not { IsAuthenticated: true })
            return Result.Failure<(Guid, List<string>)>(QuestionErrors.QuestionAccessDenied);

        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Result.Failure<(Guid, List<string>)>(QuestionErrors.QuestionAccessDenied);

        return Result.Success((userId, ReadRoles(user)));
    }

    private async Task<Result> CanManagePollAsync(Guid pollId, Guid userId, IEnumerable<string> roles, CancellationToken cancellationToken)
    {
        if (HasAnyRole(roles, DefaultRoles.Admin, DefaultRoles.SystemAdmin))
            return Result.Success();

        if (!HasAnyRole(roles, DefaultRoles.PartnerCompany))
            return Result.Failure(QuestionErrors.QuestionAccessDenied);

        var ownsPoll = await _unitOfWork.Repository<PollOwner>()
            .AnyAsync(x => x.PollId == pollId && x.UserId == userId, cancellationToken);

        return ownsPoll ? Result.Success() : Result.Failure(QuestionErrors.QuestionAccessDenied);
    }

    private static List<string> ReadRoles(ClaimsPrincipal user)
    {
        var rolesClaim = user.Claims.FirstOrDefault(x => x.Type == "roles")?.Value;

        if (string.IsNullOrWhiteSpace(rolesClaim))
            return [];

        try
        {
            var parsed = JsonSerializer.Deserialize<List<string>>(rolesClaim);
            return parsed ?? [];
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
