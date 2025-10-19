using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Survey_Basket.Infrastructure.Abstraction.Const;

public static class DefaultRoles
{
    public const string Admin = nameof(Admin);
    public const string AdminRoleId = "0199fd4b-3c1f-7eab-84c4-47f5aad20c86";
    public const string AdminRoleConcurrencyStamp = "1199fd50-a461-715e-ae9c-ddf7d6ea249f";


    public const string Member = nameof(Member);
    public const string MemberRoleId = "0299fd4b-3c1f-7eab-84c4-47f5aad20c87";
    public const string MemberRoleConcurrencyStamp = "2299fd50-a461-715e-ae9c-ddf7d6ea249f";


}
