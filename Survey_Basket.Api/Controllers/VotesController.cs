using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Votes;
using Survey_Basket.Application.Extensions;
using Survey_Basket.Application.Services.QuestionServices;
using Survey_Basket.Application.Services.VoteServices;

namespace Survey_Basket.Api.Controllers;

[Route("api/polls/{pollId:guid}/[controller]")]
[ApiController]
[Authorize]
public class VotesController : ControllerBase
{
    private readonly IQuestionService _questionService;
    private readonly IVoteService _voteService;
    public VotesController(IQuestionService questionService, IVoteService voteService)
    {
        _voteService = voteService;
        _questionService = questionService;
    }
    [HttpGet("")]
    public IActionResult Start([FromRoute] Guid pollId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var result = _questionService.GetAvailableQuestionsAsync(pollId, userId, cancellationToken);

        return result.Result.IsSuccess ? Ok(result.Result.Value) : result.Result.ToProblemDetails();
    }

    [HttpPost("")]
    public async Task<IActionResult> Submit([FromRoute] Guid pollId, [FromBody] VoteRequest request, CancellationToken cancellationToken)
    {
        var result = await _voteService.AddAsync(pollId, User.GetUserId(), request, cancellationToken);
        return result.IsSuccess
            ? Created() : result.ToProblemDetails();
    }
}
