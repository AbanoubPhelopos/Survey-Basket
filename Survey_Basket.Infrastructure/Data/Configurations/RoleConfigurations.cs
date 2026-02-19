namespace Survey_Basket.Infrastructure.Data.Configurations;

public class RoleConfigurations : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {

        builder.HasData([
            new ApplicationRole
            {
                Id = Guid.Parse(DefaultRoles.AdminRoleId),
                Name = DefaultRoles.Admin,
                NormalizedName = DefaultRoles.Admin.ToUpper(),
                ConcurrencyStamp = DefaultRoles.AdminRoleConcurrencyStamp
            },
            new ApplicationRole
            {
                Id = Guid.Parse(DefaultRoles.MemberRoleId),
                Name = DefaultRoles.Member,
                NormalizedName = DefaultRoles.Member.ToUpper(),
                ConcurrencyStamp = DefaultRoles.MemberRoleConcurrencyStamp,
                IsDefault = true,
            },
            new ApplicationRole
            {
                Id = Guid.Parse(DefaultRoles.SystemAdminRoleId),
                Name = DefaultRoles.SystemAdmin,
                NormalizedName = DefaultRoles.SystemAdmin.ToUpper(),
                ConcurrencyStamp = DefaultRoles.SystemAdminRoleConcurrencyStamp,
                IsDefault = false,
            },
            new ApplicationRole
            {
                Id = Guid.Parse(DefaultRoles.PartnerCompanyRoleId),
                Name = DefaultRoles.PartnerCompany,
                NormalizedName = DefaultRoles.PartnerCompany.ToUpper(),
                ConcurrencyStamp = DefaultRoles.PartnerCompanyRoleConcurrencyStamp,
                IsDefault = false,
            },
            new ApplicationRole
            {
                Id = Guid.Parse(DefaultRoles.CompanyUserRoleId),
                Name = DefaultRoles.CompanyUser,
                NormalizedName = DefaultRoles.CompanyUser.ToUpper(),
                ConcurrencyStamp = DefaultRoles.CompanyUserRoleConcurrencyStamp,
                IsDefault = false,
            }

            ]
        );

    }
}
