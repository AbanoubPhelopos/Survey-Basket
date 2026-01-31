namespace Survey_Basket.Infrastructure.Persistence.Repositories;

public class UserRepository(ApplicationDbContext context) : BaseRepository<ApplicationUser>(context), IUserRepository
{
    public async Task<IEnumerable<UserWithRolesResult>> GetUsersWithRolesAsync(CancellationToken cancellationToken = default)
    {
        return await (from u in _context.Users
                      join ur in _context.UserRoles
                          on u.Id equals ur.UserId
                      join r in _context.Roles
                          on ur.RoleId equals r.Id into roles
                      where !roles.Any(x => x.Name == DefaultRoles.Member)
                      select new
                      {
                          u.Id,
                          u.FirstName,
                          u.LastName,
                          u.Email,
                          u.IsDisabled,
                          Roles = roles.Select(x => x.Name!).ToList()
                      }
            ).GroupBy(u => new
            { u.Id, u.FirstName, u.LastName, u.Email, u.IsDisabled })
            .Select(u => new UserWithRolesResult(
                    u.Key.Id,
                    u.Key.FirstName,
                    u.Key.LastName,
                    u.Key.Email!,
                    u.Key.IsDisabled,
                    u.SelectMany(x => x.Roles)
                ))
            .ToListAsync(cancellationToken);
    }
}
