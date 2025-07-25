﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Survey_Basket.Domain.Models;

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

    }
}