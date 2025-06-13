using Microsoft.EntityFrameworkCore;
using Survey_Basket.Domain.Models;

namespace Survey_Basket.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Poll> Polls { get; set; } = null!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

    }
}
