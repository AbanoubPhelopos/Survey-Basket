using Survey_Basket.Domain.Models;

namespace Survey_Basket.Application.Abstraction;

public interface IJwtProvider
{
    (string Token, int ExpiresIn) GenerateToken(ApplicationUser user);
}
