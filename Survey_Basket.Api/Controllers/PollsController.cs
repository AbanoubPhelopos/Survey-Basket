using Microsoft.AspNetCore.Mvc;
using Survey_Basket.Application.Abstraction;
using Survey_Basket.Domain.Models;

namespace Survey_Basket.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PollsController : ControllerBase
    {
        private readonly IPollService _pollService;

        public PollsController(IPollService pollService)
        {
            _pollService = pollService;
        }

        [HttpGet]
        public Task<IActionResult> GetPolls()
        {
            var polls = _pollService.GetPolls();
            return Task.FromResult<IActionResult>(Ok(polls));
        }

        [HttpGet("{id}")]
        public Task<IActionResult> GetPoll(Guid id)
        {
            var poll = _pollService.GetPoll(id);
            return poll is null ?
                Task.FromResult<IActionResult>(NotFound()) :
                Task.FromResult<IActionResult>(Ok(poll));
        }
        [HttpPost]
        public Task<IActionResult> CreatePoll([FromBody] Poll poll)
        {
            if (poll == null)
            {
                return Task.FromResult<IActionResult>(BadRequest("Poll cannot be null"));
            }
            _pollService.CreatePoll(poll);
            return Task.FromResult<IActionResult>(CreatedAtAction(nameof(GetPoll), new { id = poll.Id }, poll));
        }
        [HttpDelete("{id}")]
        public Task<IActionResult> DeletePoll(Guid id)
        {
            var isDeleted = _pollService.DeletePoll(id);
            return isDeleted ? Task.FromResult<IActionResult>(NoContent()) : Task.FromResult<IActionResult>(NotFound());
        }
        [HttpPut("{id}")]
        public Task<IActionResult> UpdatePoll(Guid id, [FromBody] Poll updatedPoll)
        {
            var isUpdated = _pollService.UpdatePoll(id, updatedPoll);
            return isUpdated ? Task.FromResult<IActionResult>(NoContent()) : Task.FromResult<IActionResult>(NotFound());
        }
    }
}
