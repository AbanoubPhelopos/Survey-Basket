using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Survey_Basket.Application.Contracts.User;

public record UserProfileResponse(
    string FirstName,
    string LastName,
    string Email
);