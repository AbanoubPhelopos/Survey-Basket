using Survey_Basket.Application.Abstraction;

namespace Survey_Basket.Application.Errors;

public static class UserErrors
{
    public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "Invalid email/password");
    public static readonly Error UserNotFound = new("User.NotFound", "User not found");
    public static readonly Error RefreshTokenNotFound = new("User.RefreshTokenNotFound", "Refresh token not found or expired");
    public static readonly Error RefreshTokenInvalid = new("User.RefreshTokenInvalid", "Refresh token is invalid or has been revoked");
}
