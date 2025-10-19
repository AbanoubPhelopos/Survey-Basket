using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Survey_Basket.Infrastructure.Abstraction.Const;

namespace SurveyBasket.Persistence.EntitiesConfigurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<Guid>> builder)
    {
        //Default Data
        builder.HasData(new IdentityUserRole<Guid>
        {
            UserId = Guid.Parse(DefaultUsers.AdminId),
            RoleId = Guid.Parse(DefaultRoles.AdminRoleId)
        });
    }
}