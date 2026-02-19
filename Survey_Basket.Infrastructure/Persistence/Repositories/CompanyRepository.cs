namespace Survey_Basket.Infrastructure.Persistence.Repositories;

public class CompanyRepository(ApplicationDbContext context) : BaseRepository<Company>(context), ICompanyRepository
{
    public async Task<IEnumerable<Company>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Companies
            .Where(x => x.IsActive)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
