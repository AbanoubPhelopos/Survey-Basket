namespace Survey_Basket.Application.Contracts.Votes;

public record VoteAnswerRequest(
    Guid QuestionId,
    Guid AnswerId
    );