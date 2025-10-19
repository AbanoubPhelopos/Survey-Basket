using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Survey_Basket.Application.Abstraction;
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
}
