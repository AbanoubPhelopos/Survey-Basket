namespace Survey_Basket.Infrastructure.Contracts.Responses;

public sealed record PollResponse(
    Guid Id,
    string Title,
    string Description
    );
