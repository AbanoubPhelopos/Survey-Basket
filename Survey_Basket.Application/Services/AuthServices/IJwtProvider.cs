using Survey_Basket.Domain.Models;

namespace Survey_Basket.Application.Services.AuthServices;

public interface IJwtProvider
{
    (string Token, int ExpiresIn) GenerateToken(ApplicationUser user);
    string? ValidateToken(string token);
}
