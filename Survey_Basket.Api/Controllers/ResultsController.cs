using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey_Basket.Application.Abstractions;
using Survey_Basket.Application.Abstractions.Const;
using Survey_Basket.Application.Services.ResultServices;

namespace Survey_Basket.Api.Controllers
{
    [Route("api/polls/{pollId:guid}/[controller]")]
    [ApiController]
    [Authorize]
    public class ResultsController(IResultService resultService) : ControllerBase
    {
        private readonly IResultService _resultService = resultService;

        [HttpGet("row-data")]
        public async Task<IActionResult> GetResults([FromRoute] Guid pollId, CancellationToken cancellationToken)
        {
            var result = await _resultService.GetPollVotesAsync(pollId, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblemDetails();
        }

        [HttpGet("votes-per-day")]
        public async Task<ActionResult> VotesPerDay([FromRoute] Guid pollId, CancellationToken cancellationToken)
        {
            var result = await _resultService.GetPollVotesPerDayAsync(pollId, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblemDetails();
        }

        [HttpGet("votes-per-question")]
        public async Task<ActionResult> VotesPerQuestion([FromRoute] Guid pollId, CancellationToken cancellationToken)
        {
            var result = await _resultService.GetPollVotesPerQuestionAsync(pollId, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblemDetails();
        }

        [HttpGet("analytics")]
        public async Task<ActionResult> Analytics([FromRoute] Guid pollId, CancellationToken cancellationToken)
        {
            var result = await _resultService.GetPollAnalyticsAsync(pollId, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblemDetails();
        }
    }
}
