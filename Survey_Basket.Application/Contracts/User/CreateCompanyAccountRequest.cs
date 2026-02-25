namespace Survey_Basket.Application.Contracts.User;

public sealed record CreateCompanyAccountRequest(
    string CompanyName,
    string ContactEmail,
    string FirstName,
    string LastName,
    string? WebsiteUrl,
    string? LinkedInUrl,
    string? LogoUrl
);
