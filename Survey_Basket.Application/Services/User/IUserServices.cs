using Survey_Basket.Application.Abstractions;
using Survey_Basket.Application.Contracts.User;

namespace Survey_Basket.Application.Services.User;

public interface IUserServices
{
    Task<Result<UserProfileResponse>> GetUserProfile(Guid userId);
    Task<Result> UpdateUserProfile(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<Result> ChangePassword(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserResponse>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<Result<CreateCompanyAccountResponse>> CreateCompanyAccountAsync(Guid adminUserId, CreateCompanyAccountRequest request, CancellationToken cancellationToken = default);
    Task<Result<string>> GenerateCompanyActivationTokenAsync(Guid adminUserId, Guid companyAccountUserId, CancellationToken cancellationToken = default);
}
