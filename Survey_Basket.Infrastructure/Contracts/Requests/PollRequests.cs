namespace Survey_Basket.Infrastructure.Contracts.Requests;

public sealed record CreatePollRequests(
    string Title,
    string Description
    );

