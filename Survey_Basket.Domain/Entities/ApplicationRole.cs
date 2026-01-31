using Microsoft.AspNetCore.Identity;

namespace Survey_Basket.Domain.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public bool IsDefault { get; set; }
    public bool IsDeleted { get; set; }
}
