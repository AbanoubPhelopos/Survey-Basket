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

    public static readonly Error EmailAlreadyExists = new("User.EmailAlreadyExists", "Email already exists", StatusCodes.Status409Conflict);
    public static readonly Error UserCreationFailed = new("User.CreationFailed", "User creation failed", StatusCodes.Status500InternalServerError);

    public static readonly Error InvalidCode = new("User.InvalidCode", "The provided code is invalid or has expired.", StatusCodes.Status401Unauthorized);
    public static readonly Error EmailAlreadyConfirmed = new("User.EmailAlreadyConfirmed", "Email is already confirmed.", StatusCodes.Status400BadRequest);


}
