using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey_Basket.Application.Abstractions.Const;
using Survey_Basket.Application.Contracts.Polls;
using Survey_Basket.Application.Services.AuthServices.Filter;
using Survey_Basket.Application.Services.PollServices;


using Survey_Basket.Application.Contracts.Common;

namespace Survey_Basket.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PollsController(IPollService pollService) : ControllerBase
{
    private readonly IPollService _pollService = pollService;

    [HttpGet]
    [HasPermission(Permissions.GetPolls)]
    public async Task<IActionResult> GetPolls([FromQuery] RequestFilters filters, CancellationToken cancellationToken)
    {
        var result = await _pollService.Get(filters, cancellationToken);
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
}
