using Microsoft.AspNetCore.Identity;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Authentication;
using Survey_Basket.Domain.Models;

namespace Survey_Basket.Application.Implementation;
public class AuthService(UserManager<ApplicationUser> userManager) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
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

        ///generate token


        return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, "", 3600);
    }
}
