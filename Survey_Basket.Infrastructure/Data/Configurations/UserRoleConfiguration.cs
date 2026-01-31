namespace Survey_Basket.Infrastructure.Data.Configurations;

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