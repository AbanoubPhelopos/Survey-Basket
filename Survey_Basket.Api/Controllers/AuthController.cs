using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Authentication;

namespace Survey_Basket.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService, IOptions<JwtOptions> jwtOptions) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly JwtOptions _jwtOptions = jwtOptions.Value;


        [HttpPost("")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var authResault = await _authService.GetTokenAsync(request.Email, request.Password, cancellationToken);

            return authResault.IsSuccess
                ? Ok(authResault.Value)
                : BadRequest(authResault.Error);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var authResault = await _authService.GetRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);

            return authResault is not null
                ? Ok(authResault)
                : BadRequest(new { message = "Invalid Token" });
        }

        [HttpPut("revoke-refresh-token")]
        public async Task<IActionResult> RevokeRefreshTokenAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var isRevoked = await _authService.RevokeRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);

            return isRevoked
                ? Ok()
                : BadRequest(new { message = "Operation Field" });
        }
    }
}
