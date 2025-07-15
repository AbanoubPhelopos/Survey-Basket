using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Authentication;
using Survey_Basket.Domain.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Survey_Basket.Application.Implementation;

public class JwtProvider(IOptions<JwtOptions> jwtOptions) : IJwtProvider
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

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

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key!));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var expiresIn = _jwtOptions.ExpiresIn!;

        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer!,
            audience: _jwtOptions.Audience!,
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
