namespace Survey_Basket.Application.Contracts.Authentication;

public sealed record CompanyUserInviteRedeemRequest(
    string Token,
    string? Email,
    string? Mobile,
    string FirstName,
    string LastName,
    string? Password
);
