using Microsoft.AspNetCore.Identity;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Authentication;
using Survey_Basket.Application.Errors;
using Survey_Basket.Domain.Models;
using System.Security.Cryptography;

namespace Survey_Basket.Application.Implementation;
public class AuthService(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IJwtProvider _jwtProvider = jwtProvider;

    private readonly int _refreshTokenExpirationDays = 14;

    public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default!)
    {
        ///check user 
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        var isValidPassword = await _userManager.CheckPasswordAsync(user, password);
        if (!isValidPassword)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        var (Token, ExpiresIn) = _jwtProvider.GenerateToken(user);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays);

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            ExpiresAt = refreshTokenExpiration
        });

        await _userManager.UpdateAsync(user);

        var response = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, Token, ExpiresIn, refreshToken, refreshTokenExpiration);

        return Result.Success(response);
    }


    public async Task<AuthResponse?> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken)
    {
        var userId = _jwtProvider.ValidateToken(token);
        if (userId is null)
        {
            return null;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        var existingRefreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken && rt.IsActive);
        if (existingRefreshToken is null)
        {
            return null;
        }

        existingRefreshToken.RevokedAt = DateTime.UtcNow;

        var (Token, ExpiresIn) = _jwtProvider.GenerateToken(user);
        var newrefreshToken = GenerateRefreshToken();
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays);

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = newrefreshToken,
            ExpiresAt = refreshTokenExpiration
        });

        await _userManager.UpdateAsync(user);

        return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, Token, ExpiresIn, newrefreshToken, refreshTokenExpiration);

    }


    public async Task<bool> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken)
    {
        var userId = _jwtProvider.ValidateToken(token);
        if (userId is null)
        {
            return false;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return false;
        }

        var existingRefreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken && rt.IsActive);
        if (existingRefreshToken is null)
        {
            return false;
        }

        existingRefreshToken.RevokedAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        return true;
    }

    private static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
