namespace Survey_Basket.Application.Contracts.User;

public sealed record CreateCompanyUserInviteRequest(
    string? Email,
    string? Mobile,
    int? ExpiresInMinutes
);
