namespace Survey_Basket.Application.Contracts.Results;

public sealed record PollAnalyticsResponse(
    Guid PollId,
    string Title,
    int TotalSubmissions,
    IEnumerable<QuestionAnalyticsResponse> Questions
);
