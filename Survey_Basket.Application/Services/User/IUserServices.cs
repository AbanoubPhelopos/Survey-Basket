using Survey_Basket.Application.Abstraction;
using Survey_Basket.Application.Contracts.User;

namespace Survey_Basket.Application.Services.User;

public interface IUserServices
{
    Task<Result<UserProfileResponse>> GetUserProfile(Guid userId);
    Task<Result> UpdateUserProfile(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken);
    Task<Result> ChangePassword(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken);
}
