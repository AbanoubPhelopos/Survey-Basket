namespace Survey_Basket.Application.Contracts.User;

public sealed record AdminCompanyUserListItemResponse(
    Guid CompanyId,
    string CompanyName,
    Guid CompanyUserRecordId,
    string DisplayName,
    string BusinessIdentifier,
    bool IsLocked,
    bool IsPrimary
);
