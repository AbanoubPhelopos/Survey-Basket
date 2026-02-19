namespace Survey_Basket.Application.Contracts.Authentication;

public record AuthResponse(
    Guid UserId,
    string? Email,
    string FirstName,
    string LastName,
    string Token,
    int ExpiresIn,
    string RefreshToken,
    DateTime RefreshTokenExpiration,
    IEnumerable<string> Roles,
    IEnumerable<string> Permissions,
    string? AccountType = null,
    bool RequiresActivation = false
);
