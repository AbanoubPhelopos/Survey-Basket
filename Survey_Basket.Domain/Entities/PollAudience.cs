namespace Survey_Basket.Domain.Entities;

public sealed class PollAudience : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid PollId { get; set; }
    public Guid CompanyId { get; set; }

    public Poll Poll { get; set; } = default!;
    public Company Company { get; set; } = default!;
}
