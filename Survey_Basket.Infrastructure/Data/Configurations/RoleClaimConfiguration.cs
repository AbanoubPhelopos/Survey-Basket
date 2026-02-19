using Survey_Basket.Application.Abstractions.Const;

namespace Survey_Basket.Infrastructure.Data.Configurations;

public class RoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityRoleClaim<Guid>> builder)
    {
        var adminRoleId = Guid.Parse(DefaultRoles.AdminRoleId);
        var memberRoleId = Guid.Parse(DefaultRoles.MemberRoleId);
        var systemAdminRoleId = Guid.Parse(DefaultRoles.SystemAdminRoleId);
        var partnerCompanyRoleId = Guid.Parse(DefaultRoles.PartnerCompanyRoleId);
        var companyUserRoleId = Guid.Parse(DefaultRoles.CompanyUserRoleId);

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
            new IdentityRoleClaim<Guid> { Id = 14, RoleId = adminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.Results },
            new IdentityRoleClaim<Guid> { Id = 15, RoleId = adminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.AssignSurveyAudience },
            new IdentityRoleClaim<Guid> { Id = 16, RoleId = adminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.ManageCompanies },
            new IdentityRoleClaim<Guid> { Id = 17, RoleId = adminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.ManageCompanyUsers },
            new IdentityRoleClaim<Guid> { Id = 18, RoleId = adminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.ViewPartnerSurveyAnalytics },

            new IdentityRoleClaim<Guid> { Id = 19, RoleId = systemAdminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.GetPolls },
            new IdentityRoleClaim<Guid> { Id = 20, RoleId = systemAdminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.AddPolls },
            new IdentityRoleClaim<Guid> { Id = 21, RoleId = systemAdminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.UpdatePolls },
            new IdentityRoleClaim<Guid> { Id = 22, RoleId = systemAdminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.DeletePolls },
            new IdentityRoleClaim<Guid> { Id = 23, RoleId = systemAdminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.AssignSurveyAudience },
            new IdentityRoleClaim<Guid> { Id = 24, RoleId = systemAdminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.ManageCompanies },
            new IdentityRoleClaim<Guid> { Id = 25, RoleId = systemAdminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.ManageCompanyUsers },
            new IdentityRoleClaim<Guid> { Id = 26, RoleId = systemAdminRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.Results },

            new IdentityRoleClaim<Guid> { Id = 27, RoleId = partnerCompanyRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.ManageOwnSurveys },
            new IdentityRoleClaim<Guid> { Id = 28, RoleId = partnerCompanyRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.ViewPartnerSurveyAnalytics },
            new IdentityRoleClaim<Guid> { Id = 29, RoleId = companyUserRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.SubmitCompanySurvey },
            new IdentityRoleClaim<Guid> { Id = 30, RoleId = partnerCompanyRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.GetPolls },
            new IdentityRoleClaim<Guid> { Id = 31, RoleId = partnerCompanyRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.AddPolls },
            new IdentityRoleClaim<Guid> { Id = 32, RoleId = partnerCompanyRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.UpdatePolls },
            new IdentityRoleClaim<Guid> { Id = 33, RoleId = partnerCompanyRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.AddQuestions },
            new IdentityRoleClaim<Guid> { Id = 34, RoleId = partnerCompanyRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.UpdateQuestions },
            new IdentityRoleClaim<Guid> { Id = 35, RoleId = companyUserRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.GetPolls },
            new IdentityRoleClaim<Guid> { Id = 36, RoleId = memberRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.GetPolls },
            new IdentityRoleClaim<Guid> { Id = 37, RoleId = memberRoleId, ClaimType = Permissions.Type, ClaimValue = Permissions.SubmitCompanySurvey }
        );
    }
}
