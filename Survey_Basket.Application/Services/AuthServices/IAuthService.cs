using Survey_Basket.Application.Abstractions;
using Survey_Basket.Application.Contracts.Authentication;
using Survey_Basket.Application.Contracts.User;

namespace Survey_Basket.Application.Services.AuthServices;

public interface IAuthService
{
    Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken);
    Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken);
    Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken);
    Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request);
    Task<Result> ResendConfirmEmailAsync(ResendConfirmationEmailRequest request);
    Task<Result> SendResetPasswordCode(string email);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request);
}
