using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Survey_Basket.Domain.Models;
using Survey_Basket.Infrastructure.Abstraction.Const;

namespace Survey_Basket.Infrastructure.Persistence;

public class UserConfigurations : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FirstName)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(u => u.LastName)
            .HasMaxLength(50)
            .IsRequired();

        builder.OwnsMany(u => u.RefreshTokens).ToTable("RefreshTokens").WithOwner().HasForeignKey("UserId");

        //var passwordHasher = new PasswordHasher<ApplicationUser>();

        ///Default data 
        builder.HasData(new ApplicationUser
        {
            Id = Guid.Parse(DefaultUsers.AdminId),
            FirstName = "System",
            LastName = "Admin",
            UserName = DefaultUsers.AdminEmail,
            NormalizedUserName = DefaultUsers.AdminEmail.ToUpper(),
            Email = DefaultUsers.AdminEmail,
            NormalizedEmail = DefaultUsers.AdminEmail.ToUpper(),
            SecurityStamp = DefaultUsers.AdminSecurityStamp,
            ConcurrencyStamp = DefaultUsers.AdminConcurrencyStamp,
            EmailConfirmed = true,
            PasswordHash = DefaultUsers.AdminPasswordHash
        });
    }
}