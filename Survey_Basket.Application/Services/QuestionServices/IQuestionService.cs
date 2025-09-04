using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Question;

namespace Survey_Basket.Application.Services.QuestionServices;

public interface IQuestionService
{
    Task<Result<IEnumerable<QuestionResponse>>> GetQuestionsAsync(Guid pollId, CancellationToken cancellationToken);
    Task<Result<IEnumerable<QuestionResponse>>> GetAvailableQuestionsAsync(Guid pollId, Guid userId, CancellationToken cancellationToken);
    Task<Result<QuestionResponse>> GetQuestionAsync(Guid pollId, Guid Id, CancellationToken cancellationToken);
    Task<Result<QuestionResponse>> CreateQuestionAsync(Guid pollId, QuestionRequest request, CancellationToken cancellationToken);

    Task<Result> ToggleStatusAsync(Guid pollId, Guid Id, CancellationToken cancellationToken);

    Task<Result> UpdateAsync(Guid pollId, Guid Id, QuestionRequest request, CancellationToken cancellationToken);
}
