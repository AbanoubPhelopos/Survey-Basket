namespace Survey_Basket.Infrastructure.Persistence.Repositories;

public class PollAudienceRepository(ApplicationDbContext context) : BaseRepository<PollAudience>(context), IPollAudienceRepository
{
    public async Task<IEnumerable<PollAudience>> GetByPollIdAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        return await _context.PollAudiences
            .Where(x => x.PollId == pollId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
