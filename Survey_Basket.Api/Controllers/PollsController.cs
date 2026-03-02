using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using Survey_Basket.Api.Models;
using Survey_Basket.Application.Abstractions.Const;
using Survey_Basket.Application.Contracts.Common;
using Survey_Basket.Application.Contracts.Polls;
using Survey_Basket.Application.Extensions;
using Survey_Basket.Application.Services.AuthServices.Filter;
using Survey_Basket.Application.Services.PollServices;

namespace Survey_Basket.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PollsController(IPollService pollService, ILogger<PollsController> logger) : ControllerBase
{
    private readonly IPollService _pollService = pollService;
    private readonly ILogger<PollsController> _logger = logger;

    [HttpGet]
    [HasPermission(Permissions.GetPolls)]
    public async Task<ActionResult<ServiceResult<ServiceListResult<PollResponse, PollStatsResponse>>>> GetPolls([FromQuery] RequestFilters filters, [FromQuery] string? status, CancellationToken cancellationToken)
    {
        try
        {
            var roles = ReadRoles();
            var userId = User.GetUserId();
            var result = await _pollService.GetFilterResult(filters, status, userId, roles, cancellationToken);

            return result.IsSuccess
                ? Ok(ServiceResult<ServiceListResult<PollResponse, PollStatsResponse>>.Success(result.Value))
                : Ok(ServiceResult<ServiceListResult<PollResponse, PollStatsResponse>>.Failed(new ServiceError(result.Error.Message, int.TryParse(result.Error.Code, out var c) ? c : (result.Error.statusCode ?? 400))));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering polls");
            return Ok(ServiceResult<ServiceListResult<PollResponse, PollStatsResponse>>.Failed(new ServiceError(ex.Message, 500)));
        }
    }

    [HttpGet("stats")]
    [HasPermission(Permissions.GetPolls)]
    public async Task<IActionResult> GetPollStats(CancellationToken cancellationToken)
    {
        var roles = ReadRoles();
        var userId = User.GetUserId();
        var result = await _pollService.GetStats(userId, roles, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails();
    }

    [HttpGet("current")]
    [Authorize]
    public async Task<IActionResult> GetCurrentPolls(CancellationToken cancellationToken)
    {
        var result = await _pollService.GetCurrent(cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPoll([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _pollService.Get(id, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails();
    }

    [HttpPost]
    [HasPermission(Permissions.AddPolls)]
    public async Task<IActionResult> CreatePoll([FromBody] CreatePollRequests request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Problem("Poll cannot be null", statusCode: 400, title: "Bad Request",
                extensions: new Dictionary<string, object?>
                {
                    { "error", new[] { "Null request" } }
                });
        }

        var result = await _pollService.CreatePollAsync(request, cancellationToken);
        return result.IsSuccess
            ? Ok()
            : result.ToProblemDetails();
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.DeletePolls)]
    public async Task<IActionResult> DeletePoll([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _pollService.DeletePoll(id, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.ToProblemDetails();
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.UpdatePolls)]
    public async Task<IActionResult> UpdatePoll([FromRoute] Guid id, [FromBody] UpdatePollRequests updatedPoll, CancellationToken cancellationToken)
    {
        var result = await _pollService.UpdatePoll(id, updatedPoll, cancellationToken);

        return result.IsSuccess ?
            NoContent() : result.ToProblemDetails();
    }

    [HttpPut("{id:guid}/audience")]
    [HasPermission(Permissions.AssignSurveyAudience)]
    public async Task<IActionResult> AssignAudience([FromRoute] Guid id, [FromBody] AssignPollAudienceRequest request, CancellationToken cancellationToken)
    {
        var result = await _pollService.AssignAudienceAsync(id, request, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.ToProblemDetails();
    }

    [HttpPut("{id:guid}/togglePublish")]
    public async Task<IActionResult> TogglePublish([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _pollService.TogglePublishStatusAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    private List<string> ReadRoles()
    {
        var roles = new List<string>();

        var rolesClaim = User.Claims.FirstOrDefault(x => x.Type == "roles")?.Value;
        if (!string.IsNullOrWhiteSpace(rolesClaim))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<List<string>>(rolesClaim);
                if (parsed is { Count: > 0 })
                    roles.AddRange(parsed.Where(x => !string.IsNullOrWhiteSpace(x))!);
            }
            catch
            {
                roles.Add(rolesClaim);
            }
        }

        var roleClaims = User.Claims
            .Where(x => x.Type == ClaimTypes.Role || x.Type == "role")
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x));

        roles.AddRange(roleClaims);

        return roles
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
