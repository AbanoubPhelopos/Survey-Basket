namespace Survey_Basket.Application.Contracts.User;

public sealed record CompanyMagicLoginLinkResponse(
    Guid CompanyAccountUserId,
    string LoginUrl,
    string QrPayload,
    DateTime ExpiresOn
);
