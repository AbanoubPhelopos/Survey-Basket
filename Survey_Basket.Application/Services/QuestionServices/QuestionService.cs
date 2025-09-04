using Mapster;
using Microsoft.EntityFrameworkCore;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Answer;
using Survey_Basket.Application.Contracts.Question;
using Survey_Basket.Application.Errors;
using Survey_Basket.Domain.Models;
using Survey_Basket.Infrastructure.Data;

namespace Survey_Basket.Application.Services.QuestionServices;

public class QuestionService(ApplicationDbContext context) : IQuestionService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Result<IEnumerable<QuestionResponse>>> GetQuestionsAsync(Guid pollId, CancellationToken cancellationToken)
    {
        var isPollExist = await _context.Polls.AnyAsync(p => p.Id == pollId, cancellationToken);
        if (!isPollExist)
            return Result.Failure<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);

        var questions = await _context.Questions
            .Where(x => x.PollId == pollId)
            .Include(x => x.Answers)
            /*.Select(q => new QuestionResponse(
             q.Id,
             q.Content,
             q.Answers.Select(ans => new AnswerResponse(
                 ans.Id,
                 ans.Content
                 ))
             ))*/
            .ProjectToType<QuestionResponse>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<QuestionResponse>>(questions);
    }


    public async Task<Result<QuestionResponse>> GetQuestionAsync(Guid pollId, Guid Id, CancellationToken cancellationToken)
    {

        var question = await _context.Questions
            .Where(q => q.PollId == pollId && q.Id == Id)
            .Include(q => q.Answers)
            .ProjectToType<QuestionResponse>()
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (question is null)
            return Result.Failure<QuestionResponse>(QuestionErrors.QuestionNotFound);


        return Result.Success(question.Adapt<QuestionResponse>());
    }


    public async Task<Result<QuestionResponse>> CreateQuestionAsync(Guid pollId, QuestionRequest request, CancellationToken cancellationToken)
    {
        var isPollExist = await _context.Polls.AnyAsync(p => p.Id == pollId, cancellationToken);
        if (!isPollExist)
            return Result.Failure<QuestionResponse>(PollErrors.PollNotFound);

        var IsQuestionExist = await _context.Questions
            .AnyAsync(x => x.Content == request.Content && x.PollId == pollId, cancellationToken);
        if (IsQuestionExist)
            return Result.Failure<QuestionResponse>(QuestionErrors.QuestionAlreadyExists);

        var question = request.Adapt<Question>();

        question.PollId = pollId;

        await _context.Questions.AddAsync(question, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(question.Adapt<QuestionResponse>());
    }

    public async Task<Result> UpdateAsync(Guid pollId, Guid Id, QuestionRequest request, CancellationToken cancellationToken)
    {
        var isQuestionExist = await _context.Questions
            .AnyAsync(x => x.Content == request.Content && x.PollId == pollId && x.Id != Id, cancellationToken);
        if (isQuestionExist)
            return Result.Failure(QuestionErrors.QuestionAlreadyExists);

        var question = await _context.Questions
            .Include(x => x.Answers)
            .SingleOrDefaultAsync(x => x.Id == Id && x.PollId == pollId, cancellationToken);

        if (question is null)
            return Result.Failure(QuestionErrors.QuestionNotFound);

        question.Content = request.Content;

        ///current answers 
        var currentAnswers = question.Answers.Select(x => x.Content).ToList();

        /// add new answers 
        var newAnswers = request.Answers.Except(currentAnswers).ToList();

        newAnswers.ForEach(answer =>
        {
            question.Answers.Add(new Answer { Content = answer });
        });


        question.Answers.ToList().ForEach(answer =>
        {
            answer.IsActive = request.Answers.Contains(answer.Content);
        });

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();

    }

    public async Task<Result> ToggleStatusAsync(Guid pollId, Guid Id, CancellationToken cancellationToken)
    {
        var question = await _context.Questions
            .SingleOrDefaultAsync(x => x.Id == Id && x.PollId == pollId, cancellationToken);

        if (question is null)
            return Result.Failure(QuestionErrors.QuestionNotFound);
        question.IsActive = !question.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<QuestionResponse>>> GetAvailableQuestionsAsync(Guid pollId, Guid userId, CancellationToken cancellationToken)
    {
        var hasVote = await _context.Votes.AnyAsync(x => x.PollId == pollId && x.UserId == userId, cancellationToken);

        if (hasVote)
            return Result.Failure<IEnumerable<QuestionResponse>>(VoteErrors.VoteAlreadyExists);

        var isPollExist = await _context.Polls.AnyAsync(p => p.Id == pollId && p.IsPublished && p.StartedAt <= DateOnly.FromDateTime(DateTime.UtcNow)
                            && p.EndedAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
        if (!isPollExist)
            return Result.Failure<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);

        var questions = await _context.Questions
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

        return Result.Success<IEnumerable<QuestionResponse>>(questions);
    }
}
