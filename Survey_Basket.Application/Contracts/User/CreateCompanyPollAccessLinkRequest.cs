namespace Survey_Basket.Application.Contracts.User;

public sealed record CreateCompanyPollAccessLinkRequest(
    Guid PollId,
    int? ExpiresInMinutes
);
