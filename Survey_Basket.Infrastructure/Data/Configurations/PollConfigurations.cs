namespace Survey_Basket.Infrastructure.Data.Configurations;

public class PollConfigurations : IEntityTypeConfiguration<Poll>
{
    public void Configure(EntityTypeBuilder<Poll> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Title).IsUnique();

        builder.Property(p => p.Title).HasMaxLength(200);
        builder.Property(p => p.Summary).HasMaxLength(500);

        builder.HasOne(x => x.OwnerCompany)
            .WithMany(x => x.OwnedPolls)
            .HasForeignKey(x => x.OwnerCompanyId)
            .IsRequired(false);
    }
}
