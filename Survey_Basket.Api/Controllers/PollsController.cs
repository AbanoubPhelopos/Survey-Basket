using Microsoft.AspNetCore.Mvc;
using Survey_Basket.Domain.Models;
using Survey_Basket.Infrastructure.Data;

namespace Survey_Basket.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PollsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PollsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public Task<IActionResult> GetPolls()
        {
            var polls = _context.Polls.ToList();
            return Task.FromResult<IActionResult>(Ok(polls));
        }

        [HttpGet("{id}")]
        public Task<IActionResult> GetPoll(Guid id)
        {
            var poll = _context.Polls.FirstOrDefault(p => p.Id == id);
            if (poll == null)
            {
                return Task.FromResult<IActionResult>(NotFound());
            }
            return Task.FromResult<IActionResult>(Ok(poll));
        }
        [HttpPost]
        public Task<IActionResult> CreatePoll([FromBody] Poll poll)
        {
            if (poll == null)
            {
                return Task.FromResult<IActionResult>(BadRequest("Poll cannot be null"));
            }
            _context.Polls.Add(poll);
            _context.SaveChanges();
            return Task.FromResult<IActionResult>(CreatedAtAction(nameof(GetPoll), new { id = poll.Id }, poll));
        }
        [HttpDelete("{id}")]
        public Task<IActionResult> DeletePoll(Guid id)
        {
            var poll = _context.Polls.FirstOrDefault(p => p.Id == id);
            if (poll == null)
            {
                return Task.FromResult<IActionResult>(NotFound());
            }
            _context.Polls.Remove(poll);
            _context.SaveChanges();
            return Task.FromResult<IActionResult>(NoContent());
        }
        [HttpPut("{id}")]
        public Task<IActionResult> UpdatePoll(Guid id, [FromBody] Poll updatedPoll)
        {
            if (updatedPoll == null || updatedPoll.Id != id)
            {
                return Task.FromResult<IActionResult>(BadRequest("Invalid poll data"));
            }
            var existingPoll = _context.Polls.FirstOrDefault(p => p.Id == id);
            if (existingPoll == null)
            {
                return Task.FromResult<IActionResult>(NotFound());
            }
            existingPoll.Title = updatedPoll.Title;
            _context.SaveChanges();
            return Task.FromResult<IActionResult>(NoContent());
        }
    }
}
