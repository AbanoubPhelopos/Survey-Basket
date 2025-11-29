using Microsoft.AspNetCore.Mvc;
using Survey_Basket.Application.Services.AuthServices.Filter;
using Survey_Basket.Application.Services.RoleService;
using SurveyBasket.Abstractions.Consts;

namespace Survey_Basket.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController(IRoleService roleService) : ControllerBase
    {
        private readonly IRoleService _roleService = roleService;
        [HttpGet("")]
        [HasPermission(Permissions.GetRoles)]
        public async Task<IActionResult> Get([FromQuery] bool includeDisabled, CancellationToken cancellationToken)
        {
            var roles = await _roleService.GetRoles(includeDisabled, cancellationToken);
            return Ok(roles);
        }

    }
}