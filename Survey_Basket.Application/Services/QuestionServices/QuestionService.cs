using Mapster;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Survey_Basket.Application.Abstractions;
using Survey_Basket.Application.Contracts.Answer;
using Survey_Basket.Application.Contracts.Question;
using Survey_Basket.Application.Errors;
using Survey_Basket.Domain.Abstractions;
using Survey_Basket.Domain.Entities;
using Survey_Basket.Domain.Models;

namespace Survey_Basket.Application.Services.QuestionServices;

public class QuestionService(IUnitOfWork unitOfWork, HybridCache hybridCache, ILogger<QuestionService> logger) : IQuestionService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly HybridCache _hybridCache = hybridCache;
    private readonly ILogger _logger = logger;
    private const string cacheKeyPrefix = "Poll_Questions_";

    public async Task<Result<IEnumerable<QuestionResponse>>> GetQuestionsAsync(Guid pollId, CancellationToken cancellationToken)
    {
        var isPollExist = await _unitOfWork.Repository<Poll>().AnyAsync(p => p.Id == pollId, cancellationToken);
        if (!isPollExist)
            return Result.Failure<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);

        var questions = await _unitOfWork.Repository<Question>()
            .GetAllAsync(x => x.PollId == pollId, new[] { nameof(Question.Answers) }, cancellationToken);

        var response = questions.Adapt<IEnumerable<QuestionResponse>>();

        return Result.Success(response);
    }


    public async Task<Result<QuestionResponse>> GetQuestionAsync(Guid pollId, Guid Id, CancellationToken cancellationToken)
    {

        var question = await _unitOfWork.Repository<Question>()
            .GetAsync(q => q.PollId == pollId && q.Id == Id, new[] { nameof(Question.Answers) }, cancellationToken);

        if (question is null)
            return Result.Failure<QuestionResponse>(QuestionErrors.QuestionNotFound);


        return Result.Success(question.Adapt<QuestionResponse>());
    }


    public async Task<Result<QuestionResponse>> CreateQuestionAsync(Guid pollId, QuestionRequest request, CancellationToken cancellationToken)
    {
        var isPollExist = await _unitOfWork.Repository<Poll>().AnyAsync(p => p.Id == pollId, cancellationToken);
        if (!isPollExist)
            return Result.Failure<QuestionResponse>(PollErrors.PollNotFound);

        var IsQuestionExist = await _unitOfWork.Repository<Question>()
            .AnyAsync(x => x.Content == request.Content && x.PollId == pollId, cancellationToken);
        if (IsQuestionExist)
            return Result.Failure<QuestionResponse>(QuestionErrors.QuestionAlreadyExists);

        var question = request.Adapt<Question>();

        question.PollId = pollId;

        await _unitOfWork.Repository<Question>().AddAsync(question, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _hybridCache.RemoveAsync($"{cacheKeyPrefix}{pollId}", cancellationToken);

        return Result.Success(question.Adapt<QuestionResponse>());
    }

    public async Task<Result> UpdateAsync(Guid pollId, Guid Id, QuestionRequest request, CancellationToken cancellationToken)
    {
        var isQuestionExist = await _unitOfWork.Repository<Question>()
            .AnyAsync(x => x.Content == request.Content && x.PollId == pollId && x.Id != Id, cancellationToken);
        if (isQuestionExist)
            return Result.Failure(QuestionErrors.QuestionAlreadyExists);

        var question = await _unitOfWork.Repository<Question>()
            .GetAsync(x => x.Id == Id && x.PollId == pollId, new[] { nameof(Question.Answers) }, cancellationToken);

        if (question is null)
            return Result.Failure(QuestionErrors.QuestionNotFound);

        question.Content = request.Content;

        var currentAnswers = question.Answers.Select(x => x.Content).ToList();

        var newAnswers = request.Answers.Except(currentAnswers).ToList();

        newAnswers.ForEach(answer =>
        {
            question.Answers.Add(new Answer { Content = answer });
        });


        question.Answers.ToList().ForEach(answer =>
        {
            answer.IsActive = request.Answers.Contains(answer.Content);
        });

        _unitOfWork.Repository<Question>().Update(question);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _hybridCache.RemoveAsync($"{cacheKeyPrefix}{pollId}", cancellationToken);

        return Result.Success();

    }

    public async Task<Result> ToggleStatusAsync(Guid pollId, Guid Id, CancellationToken cancellationToken)
    {
        var question = await _unitOfWork.Repository<Question>()
            .GetAsync(x => x.Id == Id && x.PollId == pollId, cancellationToken);

        if (question is null)
            return Result.Failure(QuestionErrors.QuestionNotFound);
        question.IsActive = !question.IsActive;

        _unitOfWork.Repository<Question>().Update(question);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _hybridCache.RemoveAsync($"{cacheKeyPrefix}{pollId}", cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<QuestionResponse>>> GetAvailableQuestionsAsync(Guid pollId, Guid userId, CancellationToken cancellationToken)
    {
        var hasVote = await _unitOfWork.Repository<Vote>().AnyAsync(x => x.PollId == pollId && x.UserId == userId, cancellationToken);

        if (hasVote)
            return Result.Failure<IEnumerable<QuestionResponse>>(VoteErrors.VoteAlreadyExists);

        var isPollExist = await _unitOfWork.Repository<Poll>().AnyAsync(p => p.Id == pollId && p.IsPublished && p.StartedAt <= DateOnly.FromDateTime(DateTime.UtcNow)
                            && p.EndedAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
        if (!isPollExist)
            return Result.Failure<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);



        var cacheKey = $"{cacheKeyPrefix}{pollId}";

        var questions = await _hybridCache.GetOrCreateAsync<IEnumerable<QuestionResponse>>
            (cacheKey,
            async entry =>
            {
                var entities = await _unitOfWork.Repository<Question>()
                    .GetAllAsync(q => q.PollId == pollId && q.IsActive, new[] { nameof(Question.Answers) }, cancellationToken);

                return entities
                    .Select(q => new QuestionResponse
                    (
                        q.Id,
                        q.Content,
                        q.Answers.Where(a => a.IsActive).Select(a => new AnswerResponse(a.Id, a.Content))
                    ));
            });


        /*var cachedQuestions = await _cacheService.GetAsync<IEnumerable<QuestionResponse>>(cacheKey, cancellationToken);
        var questions = null as IEnumerable<QuestionResponse>;

        if (cachedQuestions is not null)
        {
            _logger.LogInformation("Cache hit for key {CacheKey}.", cacheKey);

            questions = cachedQuestions;
        }
        else
        {
            _logger.LogInformation("Cache miss for key {CacheKey}. Fetching from database.", cacheKey);

            questions = await _context.Questions
                .Where(q => q.PollId == pollId && q.IsActive)
                .Include(x => x.Answers)
                .Select(q => new QuestionResponse
                (
                    q.Id,
                    q.Content,
                    q.Answers.Where(a => a.IsActive).Select(a => new AnswerResponse(a.Id, a.Content))
                ))
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            await _cacheService.SetAsync(cacheKey, questions, cancellationToken);
        }*/

        return Result.Success(questions);
    }
}
