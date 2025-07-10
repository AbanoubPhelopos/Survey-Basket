using Mapster;
using Microsoft.AspNetCore.Mvc;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.Polls;
using Survey_Basket.Domain.Models;

namespace Survey_Basket.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PollsController(IPollService pollService) : ControllerBase
    {
        private readonly IPollService _pollService = pollService;

        [HttpGet]
        public Task<IActionResult> GetPolls()
        {
            var polls = _pollService.GetPolls();

            /// Mapping the domain model to the response model
            var pollsResponse = polls.Adapt<IEnumerable<PollResponse>>();
            return Task.FromResult<IActionResult>(Ok(pollsResponse));
        }

        [HttpGet("{id}")]
        public Task<IActionResult> GetPoll([FromRoute] Guid id)
        {
            var poll = _pollService.GetPoll(id);

            if (poll is not null)
            {
                /// Mapping the domain model to the response model
                var pollResponse = poll.Adapt<PollResponse>();
                return Task.FromResult<IActionResult>(Ok(poll));
            }
            return Task.FromResult<IActionResult>(NotFound());
        }
        [HttpPost]
        public Task<IActionResult> CreatePoll([FromBody] CreatePollRequests poll)
        {
            if (poll == null)
            {
                return Task.FromResult<IActionResult>(BadRequest("Poll cannot be null"));
            }

            ///mapping the request to the domain model
            var requestedPoll = poll.Adapt<Poll>();
            _pollService.CreatePoll(requestedPoll);

            return Task.FromResult<IActionResult>(CreatedAtAction(nameof(GetPoll), new { id = requestedPoll.Id }, poll));
        }
        [HttpDelete("{id}")]
        public Task<IActionResult> DeletePoll([FromRoute] Guid id)
        {
            var isDeleted = _pollService.DeletePoll(id);
            return isDeleted ? Task.FromResult<IActionResult>(NoContent()) : Task.FromResult<IActionResult>(NotFound());
        }
        [HttpPut("{id}")]
        public Task<IActionResult> UpdatePoll([FromRoute] Guid id, [FromBody] Poll updatedPoll)
        {
            var isUpdated = _pollService.UpdatePoll(id, updatedPoll);
            return isUpdated ? Task.FromResult<IActionResult>(NoContent()) : Task.FromResult<IActionResult>(NotFound());
        }
    }
}
