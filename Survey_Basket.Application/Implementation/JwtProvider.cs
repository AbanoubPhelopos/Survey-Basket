using Microsoft.IdentityModel.Tokens;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Domain.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Survey_Basket.Application.Implementation;

public class JwtProvider : IJwtProvider
{
    public (string Token, int ExpiresIn) GenerateToken(ApplicationUser user)
    {
        Claim[] claims =
            [
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(JwtRegisteredClaimNames.GivenName, user.FirstName ?? string.Empty),
                new(JwtRegisteredClaimNames.FamilyName, user.LastName ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ];

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("gwvOEFH1SAscazoGhTAOtBxJR8Zn0jaH"));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var expiresIn = 3600;

        var jwtSecurityToken = new JwtSecurityToken(
            issuer: "Survey_Basket",
            audience: "Survey_Basket users",
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(expiresIn),
            signingCredentials: signingCredentials
        );

        return (
            Token: new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
            ExpiresIn: expiresIn
        );
    }
}
