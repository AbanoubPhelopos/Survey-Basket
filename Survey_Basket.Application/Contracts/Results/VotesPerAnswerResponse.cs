namespace Survey_Basket.Application.Contracts.Results;

public record VotesPerAnswerResponse(
    string Answer,
    int Count
    );