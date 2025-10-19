using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.User;
using Survey_Basket.Application.Extensions;
using Survey_Basket.Application.Services.User;
using System.Security.Claims;

namespace Survey_Basket.Api.Controllers;

[Route("me")]
[ApiController]
[Authorize]
public class AccountController(UserServices userServices) : ControllerBase
{
    private readonly UserServices _userServices = userServices;

    [HttpGet("")]
    public async Task<IActionResult> Info()
    {
        var userId = User.GetUserId();
        var result = await _userServices.GetUserProfile(userId);

        return Ok(result.Value);
    }

    [HttpPut("info")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _userServices.UpdateUserProfile(userId, request, cancellationToken);

        return NoContent();
    }
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _userServices.ChangePassword(userId, request, cancellationToken);
        return result.IsSuccess ?  NoContent()
            : result.ToProblemDetails();
    }
}
