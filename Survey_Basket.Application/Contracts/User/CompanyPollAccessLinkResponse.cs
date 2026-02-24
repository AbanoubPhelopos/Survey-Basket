namespace Survey_Basket.Application.Contracts.User;

public sealed record CompanyPollAccessLinkResponse(
    Guid LinkId,
    Guid CompanyId,
    Guid PollId,
    string JoinUrl,
    string QrPayload,
    DateTime ExpiresOn
);
