namespace Survey_Basket.Application.Contracts.User;

public sealed record CreateCompanyUserRecordResponse(
    Guid CompanyUserRecordId,
    Guid CompanyId,
    string DisplayName,
    string BusinessIdentifier,
    bool Authenticated
);
