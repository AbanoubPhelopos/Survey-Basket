namespace Survey_Basket.Infrastructure.Data.Configurations;

public class VoteAnswersConfigurations : IEntityTypeConfiguration<VoteAnswers>
{
    public void Configure(EntityTypeBuilder<VoteAnswers> builder)
    {
        builder.HasIndex(x => new { x.VoteId, x.QuestionId }).IsUnique();
    }
}