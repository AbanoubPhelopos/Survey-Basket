namespace Survey_Basket.Application.Contracts.Authentication;

public sealed record CompanyPollAccessRedeemRequest(
    string Token,
    string FirstName,
    string LastName,
    string? Email,
    string? Mobile,
    string Password
);
