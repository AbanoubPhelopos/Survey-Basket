namespace Survey_Basket.Application.Contracts.User;

public sealed record UsersStatsResponse(
    int TotalUsers,
    int ActiveUsers,
    int DisabledUsers,
    int DistinctRoles
);
