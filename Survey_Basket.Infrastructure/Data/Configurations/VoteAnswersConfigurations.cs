namespace Survey_Basket.Infrastructure.Data.Configurations;

public class VoteAnswersConfigurations : IEntityTypeConfiguration<VoteAnswers>
{
    public void Configure(EntityTypeBuilder<VoteAnswers> builder)
    {
        builder.HasIndex(x => new { x.VoteId, x.QuestionId }).IsUnique();

        builder.Property(x => x.SelectedOptionIdsJson)
            .HasMaxLength(2000);

        builder.Property(x => x.TextValue)
            .HasMaxLength(4000);

        builder.Property(x => x.FileReference)
            .HasMaxLength(1000);

        builder.Property(x => x.CountryCode)
            .HasMaxLength(3);

        builder.Property(x => x.NumberValue)
            .HasPrecision(18, 4);
    }
}
