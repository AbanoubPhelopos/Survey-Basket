namespace Survey_Basket.Application.Contracts.Polls;

public sealed record PollResponse(
    Guid Id,
    string Title,
    string Description
    );
