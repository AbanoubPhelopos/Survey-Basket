using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey_Basket.Api.Models;
using Survey_Basket.Application.Abstractions.Const;
using Survey_Basket.Application.Contracts.Question;
using Survey_Basket.Application.Contracts.Votes;
using Survey_Basket.Application.Extensions;
using Survey_Basket.Application.Services.AuthServices.Filter;
using Survey_Basket.Application.Services.QuestionServices;
using Survey_Basket.Application.Services.VoteServices;

namespace Survey_Basket.Api.Controllers;

[Route("api/polls/{pollId:guid}/[controller]")]
[ApiController]
[Authorize]
public class VotesController(IQuestionService questionService, IVoteService voteService, ILogger<VotesController> logger) : ControllerBase
{
    private readonly IQuestionService _questionService = questionService;
    private readonly IVoteService _voteService = voteService;
    private readonly ILogger<VotesController> _logger = logger;

    [HttpGet("")]
    [HasPermission(Permissions.GetPolls)]
    public async Task<ActionResult<ServiceResult<IEnumerable<QuestionResponse>>>> Start([FromRoute] Guid pollId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await _questionService.GetAvailableQuestionsAsync(pollId, userId, cancellationToken);
            return result.IsSuccess
                ? Ok(ServiceResult<IEnumerable<QuestionResponse>>.Success(result.Value))
                : Ok(ServiceResult<IEnumerable<QuestionResponse>>.Failed(new ServiceError(result.Error.Message, int.TryParse(result.Error.Code, out var c) ? c : (result.Error.statusCode ?? 400))));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading available questions for poll {PollId}", pollId);
            return Ok(ServiceResult<IEnumerable<QuestionResponse>>.Failed(new ServiceError(ex.Message, 500)));
        }
    }

    [HttpPost("")]
    [HasPermission(Permissions.SubmitCompanySurvey)]
    public async Task<IActionResult> Submit([FromRoute] Guid pollId, [FromBody] VoteRequest request, CancellationToken cancellationToken)
    {
        var result = await _voteService.AddAsync(pollId, User.GetUserId(), request, cancellationToken);
        return result.IsSuccess
            ? Created() : result.ToProblemDetails();
    }

    [HttpGet("me")]
    [HasPermission(Permissions.GetPolls)]
    public async Task<IActionResult> MyVote([FromRoute] Guid pollId, CancellationToken cancellationToken)
    {
        var result = await _voteService.GetMyVoteAsync(pollId, User.GetUserId(), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails();
    }
}
