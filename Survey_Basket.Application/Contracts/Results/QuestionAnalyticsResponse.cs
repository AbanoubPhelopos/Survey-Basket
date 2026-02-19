namespace Survey_Basket.Application.Contracts.Results;

public sealed record QuestionAnalyticsResponse(
    Guid QuestionId,
    string Question,
    string QuestionType,
    int Responses,
    IEnumerable<AnalyticsBucketResponse> Buckets
);

public sealed record AnalyticsBucketResponse(
    string Value,
    int Count
);
