namespace Survey_Basket.Infrastructure.Data.Configurations;

public class PollAudienceConfigurations : IEntityTypeConfiguration<PollAudience>
{
    public void Configure(EntityTypeBuilder<PollAudience> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.PollId, x.CompanyId }).IsUnique();

        builder.HasOne(x => x.Poll)
            .WithMany(x => x.Audiences)
            .HasForeignKey(x => x.PollId);

        builder.HasOne(x => x.Company)
            .WithMany(x => x.TargetedPolls)
            .HasForeignKey(x => x.CompanyId);
    }
}
