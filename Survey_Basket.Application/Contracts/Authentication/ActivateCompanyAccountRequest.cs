namespace Survey_Basket.Application.Contracts.Authentication;

public sealed record ActivateCompanyAccountRequest(
    Guid CompanyAccountUserId,
    string ActivationToken,
    string NewPassword
);
