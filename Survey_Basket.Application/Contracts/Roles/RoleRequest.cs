using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Survey_Basket.Application.Contracts.Roles
{
    public record RoleRequest
    (
        string Name,
        IEnumerable<string> permissions
    );
}