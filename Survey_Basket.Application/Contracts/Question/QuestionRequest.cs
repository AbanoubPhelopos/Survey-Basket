namespace Survey_Basket.Application.Contracts.Question;

public record QuestionRequest(
    string Content,
    List<string> Answers
);
