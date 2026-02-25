namespace Survey_Basket.Application.Contracts.User;

public sealed record CompanyAccountListItemResponse(
    Guid CompanyId,
    string CompanyName,
    string CompanyCode,
    bool CompanyIsActive,
    Guid CompanyAccountUserId,
    string AccountFullName,
    string ContactEmail,
    bool IsLocked,
    string AccountState,
    string LogoUrl,
    string WebsiteUrl,
    string LinkedInUrl,
    DateTime CreatedOn
);
