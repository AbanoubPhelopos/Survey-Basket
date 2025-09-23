namespace Survey_Basket.Application.Contracts.Votes;

public record VoteRequest(
    IEnumerable<VoteAnswerRequest> Answers
    );
