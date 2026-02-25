namespace Survey_Basket.Application.Contracts.Roles;

public sealed record RoleStatsResponse(
    int TotalRoles,
    int ActiveRoles,
    int DisabledRoles,
    int PermissionLinks
);
