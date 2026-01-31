namespace Survey_Basket.Infrastructure.Data.Configurations;

public class QuestionConfigurations : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.HasIndex(x => new { x.PollId, x.Content }).IsUnique();

        builder.Property(p => p.Content).HasMaxLength(1000); ;
    }
}