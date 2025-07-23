using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Authentication;

namespace Survey_Basket.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService, IOptions<JwtOptions> jwtOptions) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost("")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.GetTokenAsync(request.Email, request.Password, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error.Message, statusCode: 401, title: result.Error.Code);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.GetRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error.Message, statusCode: 400, title: result.Error.Code);
    }

    [HttpPut("revoke-refresh-token")]
    public async Task<IActionResult> RevokeRefreshTokenAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RevokeRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);
        return result.IsSuccess
            ? Ok()
            : Problem(result.Error.Message, statusCode: 400, title: result.Error.Code);
    }
}
