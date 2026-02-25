namespace Survey_Basket.Domain.Entities;

public sealed class CompanyMagicLinkToken : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresOn { get; set; }
    public DateTime? UsedOn { get; set; }
    public DateTime? RevokedOn { get; set; }

    public ApplicationUser User { get; set; } = default!;
}
