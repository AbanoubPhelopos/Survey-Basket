using Microsoft.AspNetCore.Http;
using Survey_Basket.Application.Abstraction;

namespace Survey_Basket.Application.Errors;

public static class UserErrors
{
    public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "Invalid email/password", StatusCodes.Status401Unauthorized);
    public static readonly Error UserNotFound = new("User.NotFound", "User not found", StatusCodes.Status404NotFound);
    public static readonly Error RefreshTokenNotFound = new("User.RefreshTokenNotFound", "Refresh token not found or expired", StatusCodes.Status404NotFound);
    public static readonly Error RefreshTokenInvalid = new("User.RefreshTokenInvalid", "Refresh token is invalid or has been revoked", StatusCodes.Status401Unauthorized);
    public static readonly Error InvalidToken = new("Auth.InvalidToken", "Token is invalid or expired.", StatusCodes.Status401Unauthorized);
    public static readonly Error InvalidRefreshToken = new("Auth.InvalidRefreshToken", "Refresh token is invalid or has been revoked.", StatusCodes.Status401Unauthorized);
}
