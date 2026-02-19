namespace Survey_Basket.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ConcurrentDictionary<string, object> _repositories;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        _repositories = new ConcurrentDictionary<string, object>();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public IPollRepository Polls => _polls ??= new PollRepository(_context);
    public IVoteRepository Votes => _votes ??= new VoteRepository(_context);
    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IRoleRepository Roles => _roles ??= new RoleRepository(_context);
    public ICompanyRepository Companies => _companies ??= new CompanyRepository(_context);
    public IPollAudienceRepository PollAudiences => _pollAudiences ??= new PollAudienceRepository(_context);

    private IPollRepository? _polls;
    private IVoteRepository? _votes;
    private IUserRepository? _users;
    private IRoleRepository? _roles;
    private ICompanyRepository? _companies;
    private IPollAudienceRepository? _pollAudiences;

    public IBaseRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity).Name;

        return (IBaseRepository<TEntity>)_repositories.GetOrAdd(type, _ =>
        {
            return new BaseRepository<TEntity>(_context);
        });
    }
}
