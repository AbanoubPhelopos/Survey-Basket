namespace Survey_Basket.Infrastructure.Data.Configurations;

public class PollOwnerConfigurations : IEntityTypeConfiguration<PollOwner>
{
    public void Configure(EntityTypeBuilder<PollOwner> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.PollId, x.UserId }).IsUnique();

        builder.HasOne(x => x.Poll)
            .WithMany(x => x.Owners)
            .HasForeignKey(x => x.PollId);

        builder.HasOne(x => x.User)
            .WithMany(x => x.OwnedPolls)
            .HasForeignKey(x => x.UserId);

        builder.HasOne(x => x.Company)
            .WithMany(x => x.PollOwners)
            .HasForeignKey(x => x.CompanyId)
            .IsRequired(false);

        builder.HasOne(x => x.CreatedBy)
            .WithMany()
            .HasForeignKey(x => x.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UpdatedBy)
            .WithMany()
            .HasForeignKey(x => x.UpdatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
