namespace Survey_Basket.Application.Contracts.User;

public sealed record AdminCompanyUsersStatsResponse(
    int TotalUsers,
    int LockedUsers,
    int ActiveUsers,
    int CompaniesCount
);
