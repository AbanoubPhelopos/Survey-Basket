using Survey_Basket.Application.Contracts.Answer;

namespace Survey_Basket.Application.Contracts.Question;

public record QuestionResponse(
    Guid Id,
    string Content,
    IEnumerable<AnswerResponse> Answers
    );
