using Survey_Basket.Domain.Models;

namespace Survey_Basket.Application.Abstraction;

public interface IPollService
{
    IEnumerable<Poll> GetPolls();
    Poll? GetPoll(Guid id);
    void CreatePoll(Poll poll);
    bool DeletePoll(Guid id);
    bool UpdatePoll(Guid id, Poll updatedPoll);
}
