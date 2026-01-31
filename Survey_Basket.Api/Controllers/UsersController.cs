using Microsoft.AspNetCore.Mvc;
using Survey_Basket.Application.Abstractions.Const;
using Survey_Basket.Application.Services.AuthServices.Filter;
using Survey_Basket.Application.Services.User;

namespace Survey_Basket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserServices userServices) : ControllerBase
{
    private readonly IUserServices _userServices = userServices;

    [HttpGet("")]
    [HasPermission(Permissions.GetUsers)]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken) =>
         Ok(await _userServices.GetUsersAsync(cancellationToken));
    
}