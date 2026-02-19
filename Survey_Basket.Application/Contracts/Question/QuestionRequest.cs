using Survey_Basket.Domain.Entities;

namespace Survey_Basket.Application.Contracts.Question;

public record QuestionRequest(
    string Content,
    QuestionType Type,
    bool IsRequired,
    int DisplayOrder,
    List<string> Answers
);
