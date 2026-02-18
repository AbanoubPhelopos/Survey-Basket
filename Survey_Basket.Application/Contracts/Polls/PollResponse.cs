namespace Survey_Basket.Application.Contracts.Polls;

public sealed record PollResponse(
    Guid Id,
    string Title,
    string Summary,
    bool IsPublished,
    DateOnly StartedAt,
    DateOnly? EndedAt
);
