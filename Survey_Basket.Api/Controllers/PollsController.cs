using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Polls;
using Survey_Basket.Application.Errors;
using Survey_Basket.Application.Services.PollServices;

namespace Survey_Basket.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PollsController(IPollService pollService) : ControllerBase
{
    private readonly IPollService _pollService = pollService;

    [HttpGet]
    public async Task<IActionResult> GetPolls(CancellationToken cancellationToken)
    {
        var result = await _pollService.Get(cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails(500);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPoll([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _pollService.Get(id, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails(404);
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
            : result.ToProblemDetails(409);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePoll([FromRoute] Guid id)
    {
        var result = await _pollService.DeletePoll(id);
        return result.IsSuccess
            ? NoContent()
            : result.ToProblemDetails(404);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePoll([FromRoute] Guid id, [FromBody] UpdatePollRequests updatedPoll)
    {
        var result = await _pollService.UpdatePoll(id, updatedPoll);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result.Error.Equals(PollErrors.PollNotFound)
            ? result.ToProblemDetails(404)
            : result.ToProblemDetails(409);
    }
}
