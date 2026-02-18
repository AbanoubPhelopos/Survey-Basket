namespace Survey_Basket.Application.Contracts.Polls;

public sealed record CreatePollRequests(
    string Title,
    string Summary,
    DateOnly StartedAt,
    DateOnly? EndedAt
);

public sealed record UpdatePollRequests(
    string Title,
    string Summary,
    DateOnly StartedAt,
    DateOnly? EndedAt
);
