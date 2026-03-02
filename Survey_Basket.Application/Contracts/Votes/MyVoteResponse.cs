namespace Survey_Basket.Application.Contracts.Votes;

public sealed record MyVoteAnswerResponse(
    string Question,
    string Answer
);

public sealed record MyVoteResponse(
    Guid PollId,
    string PollTitle,
    DateTime SubmittedOn,
    IEnumerable<MyVoteAnswerResponse> Answers
);
