using Survey_Basket.Domain.Models;

namespace Survey_Basket.Domain.Entities;

public class AuditableEntity
{
    public Guid CreatedById { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public Guid? UpdatedById { get; set; }
    public DateTime? UpdatedOn { get; set; }


    public ApplicationUser CreatedBy { get; set; } = default!;
    public ApplicationUser? UpdatedBy { get; set; }
}
