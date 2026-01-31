using Survey_Basket.Application.Abstractions.Const;

namespace Survey_Basket.Infrastructure.Data.Configurations;

public class RoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityRoleClaim<Guid>> builder)
    {
        var adminRoleId = Guid.Parse(DefaultRoles.AdminRoleId);

        builder.HasData(
            new IdentityRoleClaim<Guid> { Id = 1, RoleId = adminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.GetPolls },
            new IdentityRoleClaim<Guid> { Id = 2, RoleId = adminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.AddPolls },
            new IdentityRoleClaim<Guid> { Id = 3, RoleId = adminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.UpdatePolls },
            new IdentityRoleClaim<Guid> { Id = 4, RoleId = adminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.DeletePolls },
            new IdentityRoleClaim<Guid> { Id = 5, RoleId = adminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.GetQuestions },
            new IdentityRoleClaim<Guid> { Id = 6, RoleId = adminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.AddQuestions },
            new IdentityRoleClaim<Guid> { Id = 7, RoleId = adminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.UpdateQuestions },
            new IdentityRoleClaim<Guid> { Id = 8, RoleId = adminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.GetUsers },
            new IdentityRoleClaim<Guid> { Id = 9, RoleId = adminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.AddUsers },
            new IdentityRoleClaim<Guid> { Id = 10, RoleId = adminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.UpdateUsers },
            new IdentityRoleClaim<Guid> { Id = 11, RoleId = adminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.GetRoles },
            new IdentityRoleClaim<Guid> { Id = 12, RoleId = adminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.AddRoles },
            new IdentityRoleClaim<Guid> { Id = 13, RoleId = adminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.UpdateRoles },
            new IdentityRoleClaim<Guid> { Id = 14, RoleId = adminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.Results }
        );
    }
}