namespace Survey_Basket.Application.Contracts.User;

public sealed record CompanyAccountsStatsResponse(
    int TotalCompanies,
    int ActiveCompanies,
    int InactiveCompanies,
    int LockedAccounts
);
