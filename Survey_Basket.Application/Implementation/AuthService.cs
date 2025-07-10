using Microsoft.AspNetCore.Identity;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Authentication;
using Survey_Basket.Domain.Models;

namespace Survey_Basket.Application.Implementation;
public class AuthService(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IJwtProvider _jwtProvider = jwtProvider;
    public async Task<AuthResponse?> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default!)
    {
        ///check user 
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        var isValidPassword = await _userManager.CheckPasswordAsync(user, password);
        if (!isValidPassword)
        {
            return null;
        }

        var (Token, ExpiresIn) = _jwtProvider.GenerateToken(user);


        return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, Token, ExpiresIn);
    }
}
