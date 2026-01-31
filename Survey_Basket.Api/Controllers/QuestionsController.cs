using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey_Basket.Application.Abstractions.Const;
using Survey_Basket.Application.Contracts.Question;
using Survey_Basket.Application.Services.QuestionServices;

namespace Survey_Basket.Api.Controllers;

[Route("api/polls/{pollId:guid}/[controller]")]
[ApiController]
[Authorize]
public class QuestionsController(IQuestionService questionService) : ControllerBase
{
    private readonly IQuestionService _questionService = questionService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromRoute] Guid pollId, CancellationToken cancellationToken)
    {
        var result = await _questionService.GetQuestionsAsync(pollId, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails();
    }


    [HttpGet("{Id:guid}")]
    public async Task<IActionResult> Get([FromRoute] Guid pollId, [FromRoute] Guid Id, CancellationToken cancellationToken)
    {
        var result = await _questionService.GetQuestionAsync(pollId, Id, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails();
    }


    [HttpPost]
    public async Task<IActionResult> Add([FromRoute] Guid pollId, [FromBody] QuestionRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Problem("Question cannot be null", statusCode: 400, title: "Bad Request",
                extensions: new Dictionary<string, object?>
                {
                    { "error", new[] { "Null request" } }
                });
        }

        var result = await _questionService.CreateQuestionAsync(pollId, request, cancellationToken);
        return result.IsSuccess ? CreatedAtAction(nameof(Get), new { pollId, result.Value.Id }, result.Value)
            : result.ToProblemDetails();
    }

    [HttpPut("{Id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid pollId, [FromRoute] Guid Id, [FromBody] QuestionRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Problem("Question cannot be null", statusCode: 400, title: "Bad Request",
                extensions: new Dictionary<string, object?>
                {
                    { "error", new[] { "Null request" } }
                });
        }

        var result = await _questionService.UpdateAsync(pollId, Id, request, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.ToProblemDetails();
    }

    [HttpPut("{Id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleStatus([FromRoute] Guid pollId, [FromRoute] Guid Id, CancellationToken cancellationToken)
    {
        var result = await _questionService.ToggleStatusAsync(pollId, Id, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.ToProblemDetails();
    }
}
