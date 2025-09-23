namespace Survey_Basket.Application.Contracts.Results;

public record VotesPerDayResponse(
    DateOnly Date,
    int VoteCount
    );