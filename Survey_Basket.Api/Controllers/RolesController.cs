using Microsoft.AspNetCore.Mvc;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Roles;
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

        [HttpGet("{Id}")]
        [HasPermission(Permissions.GetRoles)]
        public async Task<IActionResult> Get(string Id, CancellationToken cancellationToken)
        {
            var result = await _roleService.GetRole(Id, cancellationToken);
            return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails();
        }

        [HttpPost("")]
        [HasPermission(Permissions.AddRoles)]
        public async Task<IActionResult> Create([FromBody] RoleRequest roleRequest, CancellationToken cancellationToken)
        {
            var result = await _roleService.CreateRole(roleRequest, cancellationToken);
            return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { result.Value.Id }, result.Value)
            : result.ToProblemDetails();
        }

        [HttpPut("{roleId}")]
        [HasPermission(Permissions.UpdateRoles)]
        public async Task<IActionResult> Update(string roleId, [FromBody] RoleRequest roleRequest, CancellationToken cancellationToken)
        {
            var result = await _roleService.UpdateRole(roleId, roleRequest, cancellationToken);
            return result.IsSuccess
            ? NoContent()
            : result.ToProblemDetails();
        }

        [HttpPut("{roleId}/toggle")]
        [HasPermission(Permissions.UpdateRoles)]
        public async Task<IActionResult> Toggle(string roleId, CancellationToken cancellationToken)
        {
            var result = await _roleService.ToggleRole(roleId, cancellationToken);
            return result.IsSuccess
            ? NoContent()
            : result.ToProblemDetails();
        }
    }
}