namespace Survey_Basket.Application.Contracts.User;

public sealed record CreateCompanyAccountResponse(
    Guid CompanyId,
    Guid CompanyAccountUserId,
    string ActivationToken,
    string ActivationState
);
