using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Survey_Basket.Application.Abstractions.Const;
using Survey_Basket.Application.Contracts.User;
using Survey_Basket.Application.Extensions;
using Survey_Basket.Application.Services.AuthServices.Filter;
using Survey_Basket.Application.Services.User;

namespace Survey_Basket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(IUserServices userServices) : ControllerBase
{
    private readonly IUserServices _userServices = userServices;

    [HttpGet("")]
    [HasPermission(Permissions.GetUsers)]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken) =>
         Ok(await _userServices.GetUsersAsync(cancellationToken));

    [HttpPost("company-accounts")]
    [HasPermission(Permissions.ManageCompanies)]
    public async Task<IActionResult> CreateCompanyAccount([FromBody] CreateCompanyAccountRequest request, CancellationToken cancellationToken)
    {
        var result = await _userServices.CreateCompanyAccountAsync(User.GetUserId(), request, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails();
    }

    [HttpPost("company-user-records")]
    public async Task<IActionResult> CreateCompanyUserRecord([FromBody] CreateCompanyUserRecordRequest request, CancellationToken cancellationToken)
    {
        var result = await _userServices.CreateCompanyUserRecordAsync(User.GetUserId(), request, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails();
    }
}
