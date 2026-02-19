namespace Survey_Basket.Domain.Entities;

public sealed class CompanyUser : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid UserId { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;

    public Company Company { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;
}
