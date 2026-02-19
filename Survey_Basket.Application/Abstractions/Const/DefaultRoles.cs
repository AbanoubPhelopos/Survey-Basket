using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Survey_Basket.Application.Abstractions.Const;

public static class DefaultRoles
{
    public const string Admin = nameof(Admin);
    public const string AdminRoleId = "0199fd4b-3c1f-7eab-84c4-47f5aad20c86";
    public const string AdminRoleConcurrencyStamp = "1199fd50-a461-715e-ae9c-ddf7d6ea249f";

    public const string SystemAdmin = nameof(SystemAdmin);
    public const string SystemAdminRoleId = "0399fd4b-3c1f-7eab-84c4-47f5aad20c88";
    public const string SystemAdminRoleConcurrencyStamp = "3399fd50-a461-715e-ae9c-ddf7d6ea249f";

    public const string PartnerCompany = nameof(PartnerCompany);
    public const string PartnerCompanyRoleId = "0499fd4b-3c1f-7eab-84c4-47f5aad20c89";
    public const string PartnerCompanyRoleConcurrencyStamp = "4499fd50-a461-715e-ae9c-ddf7d6ea249f";

    public const string CompanyUser = nameof(CompanyUser);
    public const string CompanyUserRoleId = "0599fd4b-3c1f-7eab-84c4-47f5aad20c8a";
    public const string CompanyUserRoleConcurrencyStamp = "5599fd50-a461-715e-ae9c-ddf7d6ea249f";


    public const string Member = nameof(Member);
    public const string MemberRoleId = "0299fd4b-3c1f-7eab-84c4-47f5aad20c87";
    public const string MemberRoleConcurrencyStamp = "2299fd50-a461-715e-ae9c-ddf7d6ea249f";


}
