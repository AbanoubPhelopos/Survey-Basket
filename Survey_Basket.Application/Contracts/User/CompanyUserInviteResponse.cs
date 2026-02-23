namespace Survey_Basket.Application.Contracts.User;

public sealed record CompanyUserInviteResponse(
    Guid InviteId,
    Guid CompanyId,
    string InviteUrl,
    string QrPayload,
    DateTime ExpiresOn,
    string? EmailHint,
    string? MobileHint,
    bool IsUsed
);
