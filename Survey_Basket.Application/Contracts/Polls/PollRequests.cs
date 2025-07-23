namespace Survey_Basket.Application.Contracts.Polls;

public sealed record CreatePollRequests(
    string Title,
    string Description
    );

public sealed record UpdatePollRequests(
    string Title,
    string Description
    );
