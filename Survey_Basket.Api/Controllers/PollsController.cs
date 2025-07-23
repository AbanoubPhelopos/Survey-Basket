using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Polls;

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
            : StatusCode(500, result.Error);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPoll([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _pollService.Get(id, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePoll([FromBody] CreatePollRequests request)
    {
        if (request is null)
            return BadRequest("Poll cannot be null");

        var result = await _pollService.CreatePollAsync(request);
        return result.IsSuccess
            ? Ok()
            : StatusCode(500, result.Error);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePoll([FromRoute] Guid id)
    {
        var result = await _pollService.DeletePoll(id);
        return result.IsSuccess
            ? NoContent()
            : NotFound(result.Error);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePoll([FromRoute] Guid id, [FromBody] UpdatePollRequests updatedPoll)
    {
        var result = await _pollService.UpdatePoll(id, updatedPoll);
        return result.IsSuccess
            ? NoContent()
            : NotFound(result.Error);
    }
}