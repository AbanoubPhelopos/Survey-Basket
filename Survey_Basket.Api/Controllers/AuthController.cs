using Microsoft.AspNetCore.Mvc;
using Survey_Basket.Application.Abstractions;
using Survey_Basket.Application.Abstractions.Const;
using Survey_Basket.Application.Contracts.Authentication;
using Survey_Basket.Application.Contracts.User;
using Survey_Basket.Application.Services.AuthServices.Filter;
using Survey_Basket.Application.Services.AuthServices;

namespace Survey_Basket.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost("")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.GetTokenAsync(request.Email, request.Password, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.GetRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails();
    }

    [HttpPut("revoke-refresh-token")]
    public async Task<IActionResult> RevokeRefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RevokeRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);
        return result.IsSuccess
            ? Ok()
            : result.ToProblemDetails();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        return Forbid();
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        var result = await _authService.ConfirmEmailAsync(request);
        return result.IsSuccess
            ? Ok()
            : result.ToProblemDetails();
    }

    [HttpPost("resend-confirmation-email")]
    public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailRequest request)
    {
        var result = await _authService.ResendConfirmEmailAsync(request);
        return result.IsSuccess
            ? Ok()
            : result.ToProblemDetails();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgetPasswordRequest request)
    {
        var result = await _authService.SendResetPasswordCode(request.Email);
        return result.IsSuccess
            ? Ok()
            : result.ToProblemDetails();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        return result.IsSuccess
            ? Ok()
            : result.ToProblemDetails();
    }

    [HttpPost("activate-company")]
    public async Task<IActionResult> ActivateCompanyAccount([FromBody] ActivateCompanyAccountRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.ActivateCompanyAccountAsync(request, cancellationToken);
        return result.IsSuccess
            ? Ok()
            : result.ToProblemDetails();
    }

    [HttpPost("company/magic-link/request")]
    [HasPermission(Permissions.ManageCompanies)]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> RequestCompanyMagicLink([FromBody] CompanyMagicLinkRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RequestCompanyMagicLinkAsync(request, cancellationToken);
        return result.IsSuccess
            ? Ok()
            : result.ToProblemDetails();
    }

    [HttpPost("company/magic-link/redeem")]
    public async Task<IActionResult> RedeemCompanyMagicLink([FromBody] CompanyMagicLinkRedeemRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RedeemCompanyMagicLinkAsync(request, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails();
    }

    [HttpPost("company-user/invite/redeem")]
    public async Task<IActionResult> RedeemCompanyUserInvite([FromBody] CompanyUserInviteRedeemRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RedeemCompanyUserInviteAsync(request, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails();
    }
}
