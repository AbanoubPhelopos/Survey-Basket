using Microsoft.AspNetCore.Mvc;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Authentication;

namespace Survey_Basket.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;


        [HttpPost("")]
        public async Task<IActionResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
        {
            var authResault = await _authService.GetTokenAsync(request.Email, request.Password, cancellationToken);

            return authResault is not null
                ? Ok(authResault)
                : BadRequest(new { message = "Invalid email or password" });
        }
    }
}
