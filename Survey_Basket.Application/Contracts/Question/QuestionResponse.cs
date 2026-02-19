using Survey_Basket.Application.Contracts.Answer;
using Survey_Basket.Domain.Entities;

namespace Survey_Basket.Application.Contracts.Question;

public record QuestionResponse(
    Guid Id,
    string Content,
    QuestionType Type,
    bool IsRequired,
    int DisplayOrder,
    IEnumerable<AnswerResponse> Answers
    );
