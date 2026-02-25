namespace Survey_Basket.Domain.Entities;

public sealed class CompanyPollAccessLink : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid PollId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresOn { get; set; }
    public DateTime? RevokedOn { get; set; }

    public Company Company { get; set; } = default!;
    public Poll Poll { get; set; } = default!;
}
