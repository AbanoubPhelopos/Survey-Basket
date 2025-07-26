using Mapster;
using Microsoft.EntityFrameworkCore;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Question;
using Survey_Basket.Application.Errors;
using Survey_Basket.Domain.Models;
using Survey_Basket.Infrastructure.Data;

namespace Survey_Basket.Application.Services.QuestionServices;

public class QuestionService(ApplicationDbContext context) : IQuestionService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Result<QuestionResponse>> CreateQuestionAsync(Guid pollId, QuestionRequest request, CancellationToken cancellationToken)
    {
        var isPollExist = await _context.Polls.AnyAsync(p => p.Id == pollId, cancellationToken);
        if (!isPollExist)
            return Result.Failure<QuestionResponse>(PollErrors.PollNotFound);

        var IsQuestionExist = await _context.Questions
            .AnyAsync(x => x.Content == request.Content && x.PollId == pollId, cancellationToken);
        if (!IsQuestionExist)
            return Result.Failure<QuestionResponse>(QuestionErrors.QuestionNotFound);

        var question = request.Adapt<Question>();

        question.PollId = pollId;
        request.Answers.ForEach(ans => question.Answers.Add(new Answer { Content = ans }));

        await _context.Questions.AddAsync(question, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(question.Adapt<QuestionResponse>());
    }
}
