using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Services.ResultServices;

namespace Survey_Basket.Api.Controllers
{
    [Route("api/polls/{pollId:guid}/[controller]")]
    [ApiController]
    [Authorize]
    public class ResultsController : ControllerBase
    {
        private readonly IResultService _resultService;
        public ResultsController(IResultService resultService)
        {
            _resultService = resultService;
        }

        [HttpGet("row-data")]
        public async Task<IActionResult> GetResults([FromRoute] Guid pollId, CancellationToken cancellationToken)
        {
            var result = await _resultService.GetPollVotesAsync(pollId, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblemDetails();
        }

        [HttpGet]
        public async Task<ActionResult> VotesPerDay([FromRoute] Guid pollId, CancellationToken cancellationToken)
        {
            var result = await _resultService.GetPollVotesPerDayAsync(pollId, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblemDetails();
        }

    }
}
