namespace Survey_Basket.Application.Contracts.Authentication;

public record AuthResponse(
    Guid UserId,
    string Token,
    DateTime ExpiresAt,
    string FirstName,
    string LastName,
    string? Email
);
