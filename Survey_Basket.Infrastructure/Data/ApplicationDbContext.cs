using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Survey_Basket.Domain.Models;
using System.Reflection;
using System.Security.Claims;

namespace Survey_Basket.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
                : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public DbSet<Answer> Answers { get; set; } = null!;
        public DbSet<Poll> Polls { get; set; } = null!;
        public DbSet<Question> Questions { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }

            base.OnModelCreating(modelBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<AuditableEntity>();

            foreach (var entry in entries)
            {
                var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Property(x => x.CreatedById).CurrentValue = currentUserId!;

                        break;
                    case EntityState.Modified:
                        entry.Property(x => x.UpdatedById).CurrentValue = currentUserId;
                        entry.Property(x => x.UpdatedOn).CurrentValue = DateTime.UtcNow;
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
