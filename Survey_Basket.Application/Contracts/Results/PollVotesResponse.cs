namespace Survey_Basket.Application.Contracts.Results;

public record PollVotesResponse(
    string Title,
    IEnumerable<VotesResponse> Votes
    );