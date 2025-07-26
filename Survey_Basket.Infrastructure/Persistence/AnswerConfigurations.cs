using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Survey_Basket.Domain.Models;

namespace Survey_Basket.Infrastructure.Persistence;

public class AnswerConfigurations : IEntityTypeConfiguration<Answer>
{
    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder.HasIndex(x => new { x.QuestionId, x.Content }).IsUnique();

        builder.Property(p => p.Content).HasMaxLength(1000);
    }
}