namespace Survey_Basket.Application.Contracts.Polls;

public sealed record PollStatsResponse(
    int TotalPolls,
    int ActivePolls,
    int DraftPolls,
    int VotesCount,
    int AnswersCount
);
