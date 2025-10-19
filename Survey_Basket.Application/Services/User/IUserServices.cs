using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.User;

namespace Survey_Basket.Application.Services.User;

public interface IUserServices
{
    Task<Result<UserProfileResponse>> GetUserProfile(Guid userId);
}
