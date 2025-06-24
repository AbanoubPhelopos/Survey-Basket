using Survey_Basket.Application.Abstraction;
using Survey_Basket.Domain.Models;
using Survey_Basket.Infrastructure.Data;

namespace Survey_Basket.Infrastructure.Implementation;

public class PollService : IPollService
{
    private readonly ApplicationDbContext _context;
    public PollService(ApplicationDbContext context)
    {
        _context = context;
    }
    public void CreatePoll(Poll poll)
    {
        _context.Polls.Add(poll);
        _context.SaveChanges();
    }

    public bool DeletePoll(Guid id)
    {
        var existingPoll = _context.Polls.SingleOrDefault(p => p.Id == id);
        if (existingPoll is null)
        {
            return false;
        }
        _context.Polls.Remove(existingPoll);
        _context.SaveChanges();
        return true;
    }

    public Poll? GetPoll(Guid id) => _context.Polls.SingleOrDefault(p => p.Id == id);

    public IEnumerable<Poll> GetPolls() => _context.Polls.ToList();

    public bool UpdatePoll(Guid id, Poll updatedPoll)
    {
        var existingPoll = _context.Polls.SingleOrDefault(p => p.Id == id);
        if (existingPoll is null)
        {
            return false;
        }
        _context.Polls.Update(existingPoll);
        _context.SaveChanges();
        return true;
    }
}
