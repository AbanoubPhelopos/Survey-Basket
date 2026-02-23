namespace Survey_Basket.Domain.Entities;

public sealed class CompanyUserInvite : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public string? EmailHint { get; set; }
    public string? MobileHint { get; set; }
    public DateTime ExpiresOn { get; set; }
    public DateTime? UsedOn { get; set; }
    public DateTime? RevokedOn { get; set; }

    public Company Company { get; set; } = default!;
}
