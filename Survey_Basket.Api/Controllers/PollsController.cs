using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey_Basket.Application.Abstractions.Const;
using Survey_Basket.Application.Contracts.Polls;
using Survey_Basket.Application.Services.AuthServices.Filter;
using Survey_Basket.Application.Services.PollServices;


namespace Survey_Basket.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PollsController(IPollService pollService) : ControllerBase
{
    private readonly IPollService _pollService = pollService;

    [HttpGet]
    [HasPermission(Permissions.GetPolls)]
    public async Task<IActionResult> GetPolls(CancellationToken cancellationToken)
    {
        var result = await _pollService.Get(cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails();
    }

    [HttpGet("current")]
    [Authorize(Roles = DefaultRoles.Member)]
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
    public async Task<IActionResult> CreatePoll([FromBody] CreatePollRequests request)
    {
        if (request is null)
        {
            return Problem("Poll cannot be null", statusCode: 400, title: "Bad Request",
                extensions: new Dictionary<string, object?>
                {
                    { "error", new[] { "Null request" } }
                });
        }

        var result = await _pollService.CreatePollAsync(request);
        return result.IsSuccess
            ? Ok()
            : result.ToProblemDetails();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePoll([FromRoute] Guid id)
    {
        var result = await _pollService.DeletePoll(id);
        return result.IsSuccess
            ? NoContent()
            : result.ToProblemDetails();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePoll([FromRoute] Guid id, [FromBody] UpdatePollRequests updatedPoll)
    {
        var result = await _pollService.UpdatePoll(id, updatedPoll);

        return result.IsSuccess ?
            NoContent() : result.ToProblemDetails();
    }

    [HttpPut("{id}/togglePublish")]
    public async Task<IActionResult> TogglePublish([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _pollService.TogglePublishStatusAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }
}
