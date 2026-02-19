using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey_Basket.Application.Abstractions.Const;
using Survey_Basket.Application.Contracts.User;
using Survey_Basket.Application.Extensions;
using Survey_Basket.Application.Services.AuthServices.Filter;
using Survey_Basket.Application.Services.User;

namespace Survey_Basket.Api.Controllers;

[Route("me")]
[ApiController]
[Authorize]
public class AccountController(IUserServices userServices) : ControllerBase
{
    private readonly IUserServices _userServices = userServices;

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
        return result.IsSuccess ? NoContent()
            : result.ToProblemDetails();
    }

    [HttpPost("company-accounts/{companyAccountUserId:guid}/activation-token")]
    [HasPermission(Permissions.ManageCompanies)]
    public async Task<IActionResult> GenerateCompanyActivationToken([FromRoute] Guid companyAccountUserId, CancellationToken cancellationToken)
    {
        var result = await _userServices.GenerateCompanyActivationTokenAsync(User.GetUserId(), companyAccountUserId, cancellationToken);
        return result.IsSuccess
            ? Ok(new { activationToken = result.Value })
            : result.ToProblemDetails();
    }
}
