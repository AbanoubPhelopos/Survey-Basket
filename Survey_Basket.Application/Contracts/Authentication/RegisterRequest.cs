namespace Survey_Basket.Application.Contracts.Authentication;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    Guid? CompanyId = null,
    bool IsCompanyAccount = false,
    string? ActivationToken = null
);
