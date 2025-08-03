using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Survey_Basket.Domain.Models;

namespace Survey_Basket.Infrastructure.Persistence;

public class VoteAnswersConfigurations : IEntityTypeConfiguration<VoteAnswers>
{
    public void Configure(EntityTypeBuilder<VoteAnswers> builder)
    {
        builder.HasIndex(x => new { x.VoteId, x.QuestionId }).IsUnique();
    }
}