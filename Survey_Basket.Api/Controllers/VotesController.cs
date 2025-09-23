using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Errors;
using Survey_Basket.Application.Services.QuestionServices;
using System.Security.Claims;

namespace Survey_Basket.Api.Controllers;

[Route("api/polls/{pollId:guid}/[controller]")]
[ApiController]
[Authorize]
public class VotesController : ControllerBase
{
    private readonly IQuestionService _questionService;
    public VotesController(IQuestionService questionService)
    {
        _questionService = questionService;
    }
    [HttpGet("")]
    public IActionResult Start([FromRoute] Guid pollId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = _questionService.GetAvailableQuestionsAsync(pollId, Guid.Parse(userId!), cancellationToken);

        if (result.Result.IsSuccess)
        {
            return Ok(result.Result.Value);
        }
        return result.Result.Error.Equals(PollErrors.PollNotFound)
            ? result.Result.ToProblemDetails(404)
            : result.Result.ToProblemDetails(409);
    }
}
