using Survey_Basket.Application.Abstractions;
using Survey_Basket.Application.Contracts.Common;
using Survey_Basket.Application.Contracts.User;

namespace Survey_Basket.Application.Services.User;

public interface IUserServices
{
    Task<Result<UserProfileResponse>> GetUserProfile(Guid userId);
    Task<Result> UpdateUserProfile(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<Result> ChangePassword(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserResponse>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<Result<ServiceListResult<UserResponse, UsersStatsResponse>>> GetUsersFilterResultAsync(RequestFilters filters, string? status, CancellationToken cancellationToken = default);
    Task<Result<UsersStatsResponse>> GetUsersStatsAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<CreateCompanyUserRecordResponse>>> GetCompanyUserRecordsAsync(Guid companyAccountUserId, CancellationToken cancellationToken = default);
    Task<Result<ServiceListResult<CreateCompanyUserRecordResponse, CompanyUserRecordsStatsResponse>>> GetCompanyUserRecordsFilterResultAsync(Guid companyAccountUserId, RequestFilters filters, string? identifierMode, CancellationToken cancellationToken = default);
    Task<Result<CompanyUserRecordsStatsResponse>> GetCompanyUserRecordsStatsAsync(Guid companyAccountUserId, CancellationToken cancellationToken = default);
    Task<Result<CreateCompanyAccountResponse>> CreateCompanyAccountAsync(Guid adminUserId, CreateCompanyAccountRequest request, CancellationToken cancellationToken = default);
    Task<Result<ServiceListResult<CompanyAccountListItemResponse, CompanyAccountsStatsResponse>>> GetCompanyAccountsFilterResultAsync(Guid actorUserId, RequestFilters filters, string? state, CancellationToken cancellationToken = default);
    Task<Result> SetCompanyAccountLockStateAsync(Guid actorUserId, Guid companyAccountUserId, bool locked, CancellationToken cancellationToken = default);
    Task<Result<ServiceListResult<AdminCompanyUserListItemResponse, AdminCompanyUsersStatsResponse>>> GetAdminCompanyUsersFilterResultAsync(Guid actorUserId, RequestFilters filters, Guid? companyId, string? status, CancellationToken cancellationToken = default);
    Task<Result<CompanyUserInviteResponse>> CreateCompanyUserInviteAsync(Guid actorUserId, CreateCompanyUserInviteRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<CompanyUserInviteResponse>>> GetCompanyUserInvitesAsync(Guid actorUserId, CancellationToken cancellationToken = default);
    Task<Result<CompanyPollAccessLinkResponse>> CreateCompanyPollAccessLinkAsync(Guid actorUserId, CreateCompanyPollAccessLinkRequest request, CancellationToken cancellationToken = default);
    Task<Result<CompanyMagicLoginLinkResponse>> GenerateCompanyMagicLoginLinkAsync(Guid adminUserId, Guid companyAccountUserId, CancellationToken cancellationToken = default);
    Task<Result<string>> GenerateCompanyActivationTokenAsync(Guid adminUserId, Guid companyAccountUserId, CancellationToken cancellationToken = default);
    Task<Result<CreateCompanyUserRecordResponse>> CreateCompanyUserRecordAsync(Guid companyAccountUserId, CreateCompanyUserRecordRequest request, CancellationToken cancellationToken = default);
    Task<Result> SetInitialPasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken = default);
}
