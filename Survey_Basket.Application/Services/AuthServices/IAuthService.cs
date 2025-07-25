﻿using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Authentication;

namespace Survey_Basket.Application.Services.AuthServices;

public interface IAuthService
{
    Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken);
    Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken);
    Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken);
}
