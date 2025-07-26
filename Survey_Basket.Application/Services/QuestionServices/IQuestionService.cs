using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Question;

namespace Survey_Basket.Application.Services.QuestionServices;

public interface IQuestionService
{
    Task<Result<QuestionResponse>> CreateQuestionAsync(Guid pollId, QuestionRequest request, CancellationToken cancellationToken);
}
