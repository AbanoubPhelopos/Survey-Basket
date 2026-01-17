namespace Survey_Basket.Application.Contracts.User;

public record UserResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsDisabled,
    IEnumerable<string> Roles);
