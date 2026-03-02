using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;
using System.Text;
using Survey_Basket.Application.Abstractions;
using Survey_Basket.Application.Abstractions.Const;
using Survey_Basket.Application.Contracts.Common;
using Survey_Basket.Application.Contracts.User;
using Survey_Basket.Application.Errors;
using Survey_Basket.Domain.Abstractions;
using Survey_Basket.Domain.Entities;

namespace Survey_Basket.Application.Services.User;

public class UserServices(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor) : IUserServices
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;


    public async Task<Result<UserProfileResponse>> GetUserProfile(Guid userId)
    {
        var user = await _userManager.Users.Where(u => u.Id == userId)
            .ProjectToType<UserProfileResponse>()
            .SingleAsync();

        return Result.Success(user);
    }
    public async Task<Result> UpdateUserProfile(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        /*var user = await _userManager.FindByIdAsync(userId.ToString());

        user = request.Adapt(user);

        var result = await _userManager.UpdateAsync(user!);*/

        await _userManager.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(u => u
                .SetProperty(u => u.FirstName, request.FirstName)
                .SetProperty(u => u.LastName, request.LastName)
                .SetProperty(u => u.ProfileCompleted, true)
                .SetProperty(u => u.IsFirstLogin, false), cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ChangePassword(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (result.Succeeded)
        {
            user.IsFirstLogin = false;
            await _userManager.UpdateAsync(user);
            return Result.Success();
        }

        var error = result.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));

    }

    public async Task<Result> SetInitialPasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        if (!user.IsFirstLogin)
            return Result.Failure(new Error("User.InitialPasswordAlreadySet", "Initial password has already been configured.", StatusCodes.Status400BadRequest));

        var remove = await _userManager.RemovePasswordAsync(user);
        if (!remove.Succeeded)
        {
            var removeError = remove.Errors.First();
            return Result.Failure(new Error(removeError.Code, removeError.Description, StatusCodes.Status400BadRequest));
        }

        var add = await _userManager.AddPasswordAsync(user, newPassword);
        if (!add.Succeeded)
        {
            var addError = add.Errors.First();
            return Result.Failure(new Error(addError.Code, addError.Description, StatusCodes.Status400BadRequest));
        }

        user.IsFirstLogin = false;
        await _userManager.UpdateAsync(user);
        return Result.Success();
    }

    public async Task<IEnumerable<UserResponse>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _unitOfWork.Users.GetUsersWithRolesAsync(cancellationToken);

        return users.Select(u => new UserResponse(
            u.Id.ToString(),
            u.FirstName,
            u.LastName,
            u.Email,
            u.IsDisabled,
            u.Roles
        ));
    }

    public async Task<Result<UsersStatsResponse>> GetUsersStatsAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users.AsNoTracking().ToListAsync(cancellationToken);
        var totalUsers = users.Count;
        var disabledUsers = users.Count(x => x.IsDisabled);
        var activeUsers = totalUsers - disabledUsers;

        var userRoleCount = await _unitOfWork.Repository<IdentityUserRole<Guid>>()
            .GetAllAsync(cancellationToken);

        var distinctRoles = userRoleCount.Select(x => x.RoleId).Distinct().Count();

        return Result.Success(new UsersStatsResponse(
            totalUsers,
            activeUsers,
            disabledUsers,
            distinctRoles));
    }

    public async Task<Result<ServiceListResult<UserResponse, UsersStatsResponse>>> GetUsersFilterResultAsync(RequestFilters filters, string? status, CancellationToken cancellationToken = default)
    {
        var users = (await GetUsersAsync(cancellationToken)).ToList();
        var normalizedStatus = status?.Trim().ToLowerInvariant();

        var filtered = users.Where(user =>
            (string.IsNullOrWhiteSpace(filters.SearchTerm)
             || user.FirstName.Contains(filters.SearchTerm, StringComparison.OrdinalIgnoreCase)
             || user.LastName.Contains(filters.SearchTerm, StringComparison.OrdinalIgnoreCase)
             || user.Email.Contains(filters.SearchTerm, StringComparison.OrdinalIgnoreCase))
            && (string.IsNullOrWhiteSpace(normalizedStatus)
                || normalizedStatus == "all"
                || (normalizedStatus == "active" && !user.IsDisabled)
                || (normalizedStatus == "disabled" && user.IsDisabled))
        );

        filtered = (filters.SortColumn?.ToLowerInvariant(), filters.SortDirection?.ToLowerInvariant()) switch
        {
            ("firstname", "desc") => filtered.OrderByDescending(x => x.FirstName),
            ("firstname", _) => filtered.OrderBy(x => x.FirstName),
            ("lastname", "desc") => filtered.OrderByDescending(x => x.LastName),
            ("lastname", _) => filtered.OrderBy(x => x.LastName),
            ("email", "desc") => filtered.OrderByDescending(x => x.Email),
            ("email", _) => filtered.OrderBy(x => x.Email),
            _ => filtered.OrderBy(x => x.FirstName)
        };

        var filteredList = filtered.ToList();
        var totalCount = filteredList.Count;
        var pageItems = filteredList
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToList();

        var paged = new PagedList<UserResponse>(pageItems, filters.PageNumber, totalCount, filters.PageSize);
        var statsResult = await GetUsersStatsAsync(cancellationToken);
        if (!statsResult.IsSuccess)
            return Result.Failure<ServiceListResult<UserResponse, UsersStatsResponse>>(statsResult.Error);

        return Result.Success(new ServiceListResult<UserResponse, UsersStatsResponse>(paged, statsResult.Value));
    }

    public async Task<Result<IEnumerable<CreateCompanyUserRecordResponse>>> GetCompanyUserRecordsAsync(Guid companyAccountUserId, CancellationToken cancellationToken = default)
    {
        var actor = await _userManager.FindByIdAsync(companyAccountUserId.ToString());
        if (actor is null)
            return Result.Failure<IEnumerable<CreateCompanyUserRecordResponse>>(UserErrors.UserNotFound);

        var isCompanyAccount = await _userManager.IsInRoleAsync(actor, DefaultRoles.PartnerCompany);
        if (!isCompanyAccount)
            return Result.Failure<IEnumerable<CreateCompanyUserRecordResponse>>(new Error("User.Forbidden", "Only company accounts can view company user records.", StatusCodes.Status403Forbidden));

        var actorCompany = await _unitOfWork.Repository<CompanyUser>()
            .GetAsync(x => x.UserId == companyAccountUserId && x.IsPrimary && x.IsActive, cancellationToken: cancellationToken);

        if (actorCompany is null)
            return Result.Failure<IEnumerable<CreateCompanyUserRecordResponse>>(new Error("Company.NotLinked", "Company account is not linked to a company.", StatusCodes.Status400BadRequest));

        var companyRecords = await _unitOfWork.Repository<CompanyUser>()
            .GetAllAsync(x => x.CompanyId == actorCompany.CompanyId && x.IsActive && !x.IsPrimary, cancellationToken);

        var recordUserIds = companyRecords.Select(x => x.UserId).ToHashSet();

        var users = await _userManager.Users
            .Where(x => recordUserIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        var records = users.Select(u => new CreateCompanyUserRecordResponse(
            u.Id,
            actorCompany.CompanyId,
            u.FirstName,
            ExtractBusinessIdentifier(u.UserName ?? string.Empty, actorCompany.CompanyId),
            !u.IsDisabled
        ));

        return Result.Success(records);
    }

    public async Task<Result<CompanyUserRecordsStatsResponse>> GetCompanyUserRecordsStatsAsync(Guid companyAccountUserId, CancellationToken cancellationToken = default)
    {
        var recordsResult = await GetCompanyUserRecordsAsync(companyAccountUserId, cancellationToken);
        if (!recordsResult.IsSuccess)
            return Result.Failure<CompanyUserRecordsStatsResponse>(recordsResult.Error);

        var records = recordsResult.Value.ToList();

        return Result.Success(new CompanyUserRecordsStatsResponse(
            records.Count,
            records.Count(x => x.BusinessIdentifier.Length <= 7),
            records.Count(x => x.BusinessIdentifier.Length > 7)
        ));
    }

    public async Task<Result<ServiceListResult<CreateCompanyUserRecordResponse, CompanyUserRecordsStatsResponse>>> GetCompanyUserRecordsFilterResultAsync(Guid companyAccountUserId, RequestFilters filters, string? identifierMode, CancellationToken cancellationToken = default)
    {
        var recordsResult = await GetCompanyUserRecordsAsync(companyAccountUserId, cancellationToken);
        if (!recordsResult.IsSuccess)
            return Result.Failure<ServiceListResult<CreateCompanyUserRecordResponse, CompanyUserRecordsStatsResponse>>(recordsResult.Error);

        var mode = identifierMode?.Trim().ToLowerInvariant();

        var filtered = recordsResult.Value.Where(record =>
            (string.IsNullOrWhiteSpace(filters.SearchTerm)
             || record.DisplayName.Contains(filters.SearchTerm, StringComparison.OrdinalIgnoreCase)
             || record.BusinessIdentifier.Contains(filters.SearchTerm, StringComparison.OrdinalIgnoreCase))
            && (string.IsNullOrWhiteSpace(mode)
                || mode == "all"
                || (mode == "short" && record.BusinessIdentifier.Length <= 7)
                || (mode == "long" && record.BusinessIdentifier.Length > 7))
        ).ToList();

        var totalCount = filtered.Count;
        var pageItems = filtered
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToList();

        var paged = new PagedList<CreateCompanyUserRecordResponse>(pageItems, filters.PageNumber, totalCount, filters.PageSize);

        var statsResult = await GetCompanyUserRecordsStatsAsync(companyAccountUserId, cancellationToken);
        if (!statsResult.IsSuccess)
            return Result.Failure<ServiceListResult<CreateCompanyUserRecordResponse, CompanyUserRecordsStatsResponse>>(statsResult.Error);

        return Result.Success(new ServiceListResult<CreateCompanyUserRecordResponse, CompanyUserRecordsStatsResponse>(paged, statsResult.Value));
    }

    public async Task<Result<CreateCompanyAccountResponse>> CreateCompanyAccountAsync(Guid adminUserId, CreateCompanyAccountRequest request, CancellationToken cancellationToken = default)
    {
        var admin = await _userManager.FindByIdAsync(adminUserId.ToString());
        if (admin is null)
            return Result.Failure<CreateCompanyAccountResponse>(UserErrors.UserNotFound);

        var isAdmin = await _userManager.IsInRoleAsync(admin, DefaultRoles.Admin) || await _userManager.IsInRoleAsync(admin, DefaultRoles.SystemAdmin);
        if (!isAdmin)
            return Result.Failure<CreateCompanyAccountResponse>(new Error("User.Forbidden", "Only admins can create company accounts.", StatusCodes.Status403Forbidden));

        var emailExists = await _userManager.Users.AnyAsync(x => x.Email == request.ContactEmail, cancellationToken);
        if (emailExists)
            return Result.Failure<CreateCompanyAccountResponse>(UserErrors.EmailAlreadyExists);

        var companyNameExists = await _unitOfWork.Repository<Company>().AnyAsync(x => x.Name == request.CompanyName, cancellationToken);
        if (companyNameExists)
            return Result.Failure<CreateCompanyAccountResponse>(new Error("Company.NameExists", "Company name already exists.", StatusCodes.Status409Conflict));

        var company = new Company
        {
            Name = request.CompanyName.Trim(),
            Code = BuildCompanyCode(request.CompanyName),
            IsActive = true,
            ContactEmail = request.ContactEmail.Trim(),
            WebsiteUrl = string.IsNullOrWhiteSpace(request.WebsiteUrl) ? null : request.WebsiteUrl.Trim(),
            LinkedInUrl = string.IsNullOrWhiteSpace(request.LinkedInUrl) ? null : request.LinkedInUrl.Trim(),
            LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl.Trim(),
            CreatedById = adminUserId
        };

        var companyCodeExists = await _unitOfWork.Repository<Company>().AnyAsync(x => x.Code == company.Code, cancellationToken);
        if (companyCodeExists)
            company.Code = BuildCompanyCode($"{request.CompanyName}-{Guid.NewGuid():N}" );

        await _unitOfWork.Repository<Company>().AddAsync(company, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var companyAccount = new ApplicationUser
        {
            Email = request.ContactEmail.Trim(),
            UserName = request.ContactEmail.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            EmailConfirmed = true,
            IsDisabled = true,
            ProfileCompleted = true,
            IsFirstLogin = true
        };

        var tempPassword = $"Tmp!{Guid.NewGuid():N}aA1";
        var createResult = await _userManager.CreateAsync(companyAccount, tempPassword);
        if (!createResult.Succeeded)
        {
            var error = createResult.Errors.First();
            return Result.Failure<CreateCompanyAccountResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        var addRoleResult = await _userManager.AddToRoleAsync(companyAccount, DefaultRoles.PartnerCompany);
        if (!addRoleResult.Succeeded)
        {
            var roleError = addRoleResult.Errors.First();
            return Result.Failure<CreateCompanyAccountResponse>(new Error(roleError.Code, roleError.Description, StatusCodes.Status400BadRequest));
        }

        await _unitOfWork.Repository<CompanyUser>().AddAsync(new CompanyUser
        {
            CompanyId = company.Id,
            UserId = companyAccount.Id,
            IsPrimary = true,
            IsActive = true,
            CreatedById = adminUserId
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var activationToken = await GenerateEncodedResetTokenAsync(companyAccount);

        return Result.Success(new CreateCompanyAccountResponse(
            company.Id,
            companyAccount.Id,
            activationToken,
            "PendingPassword"
        ));
    }

    public async Task<Result<ServiceListResult<CompanyAccountListItemResponse, CompanyAccountsStatsResponse>>> GetCompanyAccountsFilterResultAsync(Guid actorUserId, RequestFilters filters, string? state, CancellationToken cancellationToken = default)
    {
        var actor = await _userManager.FindByIdAsync(actorUserId.ToString());
        if (actor is null)
            return Result.Failure<ServiceListResult<CompanyAccountListItemResponse, CompanyAccountsStatsResponse>>(UserErrors.UserNotFound);

        if (!await IsAdminAsync(actor))
            return Result.Failure<ServiceListResult<CompanyAccountListItemResponse, CompanyAccountsStatsResponse>>(new Error("User.Forbidden", "Only admins can view company accounts.", StatusCodes.Status403Forbidden));

        var links = await _unitOfWork.Repository<CompanyUser>()
            .GetAllAsync(x => x.IsPrimary && x.IsActive, includes: ["Company", "User"], cancellationToken: cancellationToken);

        var items = links
            .Where(x => x.Company is not null && x.User is not null)
            .Select(x =>
            {
                var company = x.Company;
                var user = x.User;
                var isLocked = user.IsDisabled;
                var accountState = isLocked ? "Locked" : "Active";
                var companySlug = company.Code.Trim().ToLowerInvariant();

                return new CompanyAccountListItemResponse(
                    company.Id,
                    company.Name,
                    company.Code,
                    company.IsActive,
                    user.Id,
                    $"{user.FirstName} {user.LastName}".Trim(),
                    user.Email ?? string.Empty,
                    isLocked,
                    accountState,
                    string.IsNullOrWhiteSpace(company.LogoUrl)
                        ? $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(company.Name)}&background=0f766e&color=ffffff"
                        : company.LogoUrl,
                    string.IsNullOrWhiteSpace(company.WebsiteUrl)
                        ? $"https://{companySlug}.example.com"
                        : company.WebsiteUrl,
                    string.IsNullOrWhiteSpace(company.LinkedInUrl)
                        ? $"https://www.linkedin.com/search/results/companies/?keywords={Uri.EscapeDataString(company.Name)}"
                        : company.LinkedInUrl,
                    company.CreatedOn
                );
            })
            .ToList();

        var normalizedState = state?.Trim().ToLowerInvariant();
        var filtered = items.Where(x =>
            (string.IsNullOrWhiteSpace(filters.SearchTerm)
             || x.CompanyName.Contains(filters.SearchTerm, StringComparison.OrdinalIgnoreCase)
             || x.ContactEmail.Contains(filters.SearchTerm, StringComparison.OrdinalIgnoreCase)
             || x.CompanyCode.Contains(filters.SearchTerm, StringComparison.OrdinalIgnoreCase))
            && (string.IsNullOrWhiteSpace(normalizedState)
                || normalizedState == "all"
                || (normalizedState == "active" && !x.IsLocked)
                || (normalizedState == "locked" && x.IsLocked)
                || (normalizedState == "inactive" && !x.CompanyIsActive))
        );

        filtered = (filters.SortColumn?.ToLowerInvariant(), filters.SortDirection?.ToLowerInvariant()) switch
        {
            ("companyname", "desc") => filtered.OrderByDescending(x => x.CompanyName),
            ("companyname", _) => filtered.OrderBy(x => x.CompanyName),
            ("contactemail", "desc") => filtered.OrderByDescending(x => x.ContactEmail),
            ("contactemail", _) => filtered.OrderBy(x => x.ContactEmail),
            _ => filtered.OrderByDescending(x => x.CreatedOn)
        };

        var filteredList = filtered.ToList();
        var pagedItems = filteredList
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToList();

        var stats = new CompanyAccountsStatsResponse(
            items.Count,
            items.Count(x => x.CompanyIsActive),
            items.Count(x => !x.CompanyIsActive),
            items.Count(x => x.IsLocked)
        );

        var paged = new PagedList<CompanyAccountListItemResponse>(pagedItems, filters.PageNumber, filteredList.Count, filters.PageSize);
        return Result.Success(new ServiceListResult<CompanyAccountListItemResponse, CompanyAccountsStatsResponse>(paged, stats));
    }

    public async Task<Result> SetCompanyAccountLockStateAsync(Guid actorUserId, Guid companyAccountUserId, bool locked, CancellationToken cancellationToken = default)
    {
        var actor = await _userManager.FindByIdAsync(actorUserId.ToString());
        if (actor is null)
            return Result.Failure(UserErrors.UserNotFound);

        if (!await IsAdminAsync(actor))
            return Result.Failure(new Error("User.Forbidden", "Only admins can lock or unlock company accounts.", StatusCodes.Status403Forbidden));

        var target = await _userManager.FindByIdAsync(companyAccountUserId.ToString());
        if (target is null)
            return Result.Failure(UserErrors.UserNotFound);

        var isCompanyAccount = await _userManager.IsInRoleAsync(target, DefaultRoles.PartnerCompany);
        if (!isCompanyAccount)
            return Result.Failure(new Error("User.InvalidRole", "Target user is not a company account.", StatusCodes.Status400BadRequest));

        target.IsDisabled = locked;
        var update = await _userManager.UpdateAsync(target);
        if (!update.Succeeded)
        {
            var error = update.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        return Result.Success();
    }

    public async Task<Result<ServiceListResult<AdminCompanyUserListItemResponse, AdminCompanyUsersStatsResponse>>> GetAdminCompanyUsersFilterResultAsync(Guid actorUserId, RequestFilters filters, Guid? companyId, string? status, CancellationToken cancellationToken = default)
    {
        var actor = await _userManager.FindByIdAsync(actorUserId.ToString());
        if (actor is null)
            return Result.Failure<ServiceListResult<AdminCompanyUserListItemResponse, AdminCompanyUsersStatsResponse>>(UserErrors.UserNotFound);

        if (!await IsAdminAsync(actor))
            return Result.Failure<ServiceListResult<AdminCompanyUserListItemResponse, AdminCompanyUsersStatsResponse>>(new Error("User.Forbidden", "Only admins can view company users.", StatusCodes.Status403Forbidden));

        var links = await _unitOfWork.Repository<CompanyUser>()
            .GetAllAsync(x => !x.IsPrimary && x.IsActive && (!companyId.HasValue || x.CompanyId == companyId), includes: ["Company", "User"], cancellationToken: cancellationToken);

        var list = links
            .Where(x => x.Company is not null && x.User is not null)
            .Select(x => new AdminCompanyUserListItemResponse(
                x.CompanyId,
                x.Company.Name,
                x.UserId,
                x.User.FirstName,
                ExtractBusinessIdentifier(x.User.UserName ?? string.Empty, x.CompanyId),
                x.User.IsDisabled,
                x.IsPrimary
            ))
            .ToList();

        var normalizedStatus = status?.Trim().ToLowerInvariant();
        var filtered = list.Where(x =>
            (string.IsNullOrWhiteSpace(filters.SearchTerm)
             || x.DisplayName.Contains(filters.SearchTerm, StringComparison.OrdinalIgnoreCase)
             || x.BusinessIdentifier.Contains(filters.SearchTerm, StringComparison.OrdinalIgnoreCase)
             || x.CompanyName.Contains(filters.SearchTerm, StringComparison.OrdinalIgnoreCase))
            && (string.IsNullOrWhiteSpace(normalizedStatus)
                || normalizedStatus == "all"
                || (normalizedStatus == "active" && !x.IsLocked)
                || (normalizedStatus == "locked" && x.IsLocked))
        );

        filtered = (filters.SortColumn?.ToLowerInvariant(), filters.SortDirection?.ToLowerInvariant()) switch
        {
            ("companyname", "desc") => filtered.OrderByDescending(x => x.CompanyName),
            ("companyname", _) => filtered.OrderBy(x => x.CompanyName),
            ("displayname", "desc") => filtered.OrderByDescending(x => x.DisplayName),
            ("displayname", _) => filtered.OrderBy(x => x.DisplayName),
            _ => filtered.OrderBy(x => x.CompanyName).ThenBy(x => x.DisplayName)
        };

        var filteredList = filtered.ToList();
        var pagedItems = filteredList
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToList();

        var stats = new AdminCompanyUsersStatsResponse(
            list.Count,
            list.Count(x => x.IsLocked),
            list.Count(x => !x.IsLocked),
            list.Select(x => x.CompanyId).Distinct().Count()
        );

        var paged = new PagedList<AdminCompanyUserListItemResponse>(pagedItems, filters.PageNumber, filteredList.Count, filters.PageSize);
        return Result.Success(new ServiceListResult<AdminCompanyUserListItemResponse, AdminCompanyUsersStatsResponse>(paged, stats));
    }

    public async Task<Result<string>> GenerateCompanyActivationTokenAsync(Guid adminUserId, Guid companyAccountUserId, CancellationToken cancellationToken = default)
    {
        var admin = await _userManager.FindByIdAsync(adminUserId.ToString());
        if (admin is null)
            return Result.Failure<string>(UserErrors.UserNotFound);

        var isAdmin = await _userManager.IsInRoleAsync(admin, DefaultRoles.Admin) || await _userManager.IsInRoleAsync(admin, DefaultRoles.SystemAdmin);
        if (!isAdmin)
            return Result.Failure<string>(new Error("User.Forbidden", "Only admins can generate company activation tokens.", StatusCodes.Status403Forbidden));

        var companyAccount = await _userManager.FindByIdAsync(companyAccountUserId.ToString());
        if (companyAccount is null)
            return Result.Failure<string>(UserErrors.UserNotFound);

        var isCompanyAccount = await _userManager.IsInRoleAsync(companyAccount, DefaultRoles.PartnerCompany);
        if (!isCompanyAccount)
            return Result.Failure<string>(new Error("User.InvalidRole", "Target user is not a company account.", StatusCodes.Status400BadRequest));

        var token = await GenerateEncodedResetTokenAsync(companyAccount);
        return Result.Success(token);
    }

    public async Task<Result<CreateCompanyUserRecordResponse>> CreateCompanyUserRecordAsync(Guid companyAccountUserId, CreateCompanyUserRecordRequest request, CancellationToken cancellationToken = default)
    {
        var actor = await _userManager.FindByIdAsync(companyAccountUserId.ToString());
        if (actor is null)
            return Result.Failure<CreateCompanyUserRecordResponse>(UserErrors.UserNotFound);

        var isCompanyAccount = await _userManager.IsInRoleAsync(actor, DefaultRoles.PartnerCompany);
        if (!isCompanyAccount)
            return Result.Failure<CreateCompanyUserRecordResponse>(new Error("User.Forbidden", "Only company accounts can create company user records.", StatusCodes.Status403Forbidden));

        var actorCompany = await _unitOfWork.Repository<CompanyUser>()
            .GetAsync(x => x.UserId == companyAccountUserId && x.IsPrimary && x.IsActive, cancellationToken: cancellationToken);

        if (actorCompany is null)
            return Result.Failure<CreateCompanyUserRecordResponse>(new Error("Company.NotLinked", "Company account is not linked to a company.", StatusCodes.Status400BadRequest));

        var normalizedIdentifier = request.BusinessIdentifier.Trim().ToUpperInvariant();
        var recordUserName = BuildCompanyUserName(actorCompany.CompanyId, normalizedIdentifier);
        var duplicate = await _userManager.Users.AnyAsync(x => x.UserName == recordUserName, cancellationToken);
        if (duplicate)
            return Result.Failure<CreateCompanyUserRecordResponse>(new Error("CompanyUser.IdentifierExists", "Business identifier already exists for this company.", StatusCodes.Status409Conflict));

        var recordUser = new ApplicationUser
        {
            FirstName = request.DisplayName.Trim(),
            LastName = "Record",
            Email = $"{recordUserName}@records.local",
            UserName = recordUserName,
            EmailConfirmed = true,
            IsDisabled = true,
            ProfileCompleted = false,
            IsFirstLogin = true
        };

        var tempPassword = $"Tmp!{Guid.NewGuid():N}aA1";
        var createResult = await _userManager.CreateAsync(recordUser, tempPassword);
        if (!createResult.Succeeded)
        {
            var error = createResult.Errors.First();
            return Result.Failure<CreateCompanyUserRecordResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        var roleResult = await _userManager.AddToRoleAsync(recordUser, DefaultRoles.CompanyUser);
        if (!roleResult.Succeeded)
        {
            var roleError = roleResult.Errors.First();
            return Result.Failure<CreateCompanyUserRecordResponse>(new Error(roleError.Code, roleError.Description, StatusCodes.Status400BadRequest));
        }

        await _unitOfWork.Repository<CompanyUser>().AddAsync(new CompanyUser
        {
            CompanyId = actorCompany.CompanyId,
            UserId = recordUser.Id,
            IsPrimary = false,
            IsActive = true,
            CreatedById = companyAccountUserId
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateCompanyUserRecordResponse(
            recordUser.Id,
            actorCompany.CompanyId,
            request.DisplayName.Trim(),
            normalizedIdentifier,
            false
        ));
    }

    public async Task<Result<CompanyUserInviteResponse>> CreateCompanyUserInviteAsync(Guid actorUserId, CreateCompanyUserInviteRequest request, CancellationToken cancellationToken = default)
    {
        var actor = await _userManager.FindByIdAsync(actorUserId.ToString());
        if (actor is null)
            return Result.Failure<CompanyUserInviteResponse>(UserErrors.UserNotFound);

        var isCompanyAccount = await _userManager.IsInRoleAsync(actor, DefaultRoles.PartnerCompany);
        if (!isCompanyAccount)
            return Result.Failure<CompanyUserInviteResponse>(new Error("User.Forbidden", "Only company accounts can generate invites.", StatusCodes.Status403Forbidden));

        var actorCompany = await _unitOfWork.Repository<CompanyUser>()
            .GetAsync(x => x.UserId == actorUserId && x.IsActive, cancellationToken: cancellationToken);

        if (actorCompany is null)
            return Result.Failure<CompanyUserInviteResponse>(new Error("Company.NotLinked", "Company account is not linked to a company.", StatusCodes.Status400BadRequest));

        var emailHint = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant();
        var mobileHint = string.IsNullOrWhiteSpace(request.Mobile) ? null : request.Mobile.Trim();

        if (string.IsNullOrWhiteSpace(emailHint) && string.IsNullOrWhiteSpace(mobileHint))
            return Result.Failure<CompanyUserInviteResponse>(new Error("Invite.IdentityRequired", "Email or mobile is required to create a secure invite.", StatusCodes.Status400BadRequest));

        var rawToken = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(48));
        var tokenHash = ComputeSha256(rawToken);
        var expiresOn = DateTime.UtcNow.AddMinutes(Math.Clamp(request.ExpiresInMinutes ?? 15, 5, 60));

        var invite = new CompanyUserInvite
        {
            CompanyId = actorCompany.CompanyId,
            TokenHash = tokenHash,
            EmailHint = emailHint,
            MobileHint = mobileHint,
            ExpiresOn = expiresOn,
            CreatedById = actorUserId
        };

        await _unitOfWork.Repository<CompanyUserInvite>().AddAsync(invite, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var url = $"/join-company?token={rawToken}";

        return Result.Success(new CompanyUserInviteResponse(
            invite.Id,
            invite.CompanyId,
            url,
            url,
            invite.ExpiresOn,
            invite.EmailHint,
            invite.MobileHint,
            false
        ));
    }

    public async Task<Result<CompanyPollAccessLinkResponse>> CreateCompanyPollAccessLinkAsync(Guid actorUserId, CreateCompanyPollAccessLinkRequest request, CancellationToken cancellationToken = default)
    {
        var actor = await _userManager.FindByIdAsync(actorUserId.ToString());
        if (actor is null)
            return Result.Failure<CompanyPollAccessLinkResponse>(UserErrors.UserNotFound);

        var isCompanyAccount = await _userManager.IsInRoleAsync(actor, DefaultRoles.PartnerCompany);
        if (!isCompanyAccount)
            return Result.Failure<CompanyPollAccessLinkResponse>(new Error("User.Forbidden", "Only company accounts can generate poll QR links.", StatusCodes.Status403Forbidden));

        var actorCompany = await _unitOfWork.Repository<CompanyUser>()
            .GetAsync(x => x.UserId == actorUserId && x.IsPrimary && x.IsActive, cancellationToken: cancellationToken);

        if (actorCompany is null)
            return Result.Failure<CompanyPollAccessLinkResponse>(new Error("Company.NotLinked", "Company account is not linked to a company.", StatusCodes.Status400BadRequest));

        var poll = await _unitOfWork.Repository<Poll>()
            .GetByIdAsync(request.PollId, cancellationToken);

        if (poll is null || !poll.IsPublished)
            return Result.Failure<CompanyPollAccessLinkResponse>(PollErrors.PollNotFound);

        var targeted = await _unitOfWork.Repository<PollAudience>()
            .AnyAsync(x => x.PollId == request.PollId && x.CompanyId == actorCompany.CompanyId, cancellationToken);

        var companyOwnsPoll = poll.OwnerCompanyId.HasValue && poll.OwnerCompanyId.Value == actorCompany.CompanyId;

        if (!targeted && !companyOwnsPoll)
            return Result.Failure<CompanyPollAccessLinkResponse>(new Error("Poll.NotTargeted", "Poll is not targeted to your company.", StatusCodes.Status403Forbidden));

        var rawToken = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(48));
        var tokenHash = ComputeSha256(rawToken);
        var expiresOn = DateTime.UtcNow.AddMinutes(Math.Clamp(request.ExpiresInMinutes ?? 240, 30, 4320));

        var link = new CompanyPollAccessLink
        {
            CompanyId = actorCompany.CompanyId,
            PollId = request.PollId,
            TokenHash = tokenHash,
            ExpiresOn = expiresOn,
            CreatedById = actorUserId
        };

        await _unitOfWork.Repository<CompanyPollAccessLink>().AddAsync(link, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var joinUrl = $"/join-company-poll?token={rawToken}";

        return Result.Success(new CompanyPollAccessLinkResponse(
            link.Id,
            link.CompanyId,
            link.PollId,
            joinUrl,
            joinUrl,
            expiresOn
        ));
    }

    public async Task<Result<IEnumerable<CompanyUserInviteResponse>>> GetCompanyUserInvitesAsync(Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await _userManager.FindByIdAsync(actorUserId.ToString());
        if (actor is null)
            return Result.Failure<IEnumerable<CompanyUserInviteResponse>>(UserErrors.UserNotFound);

        var isCompanyAccount = await _userManager.IsInRoleAsync(actor, DefaultRoles.PartnerCompany);
        if (!isCompanyAccount)
            return Result.Failure<IEnumerable<CompanyUserInviteResponse>>(new Error("User.Forbidden", "Only company accounts can view invites.", StatusCodes.Status403Forbidden));

        var actorCompany = await _unitOfWork.Repository<CompanyUser>()
            .GetAsync(x => x.UserId == actorUserId && x.IsPrimary && x.IsActive, cancellationToken: cancellationToken);

        if (actorCompany is null)
            return Result.Failure<IEnumerable<CompanyUserInviteResponse>>(new Error("Company.NotLinked", "Company account is not linked to a company.", StatusCodes.Status400BadRequest));

        var invites = await _unitOfWork.Repository<CompanyUserInvite>()
            .GetAllAsync(x => x.CompanyId == actorCompany.CompanyId, cancellationToken);

        var response = invites
            .OrderByDescending(x => x.CreatedOn)
            .Select(invite => new CompanyUserInviteResponse(
                invite.Id,
                invite.CompanyId,
                string.Empty,
                string.Empty,
                invite.ExpiresOn,
                invite.EmailHint,
                invite.MobileHint,
                invite.UsedOn.HasValue
            ));

        return Result.Success(response);
    }

    public async Task<Result<CompanyMagicLoginLinkResponse>> GenerateCompanyMagicLoginLinkAsync(Guid adminUserId, Guid companyAccountUserId, CancellationToken cancellationToken = default)
    {
        var admin = await _userManager.FindByIdAsync(adminUserId.ToString());
        if (admin is null)
            return Result.Failure<CompanyMagicLoginLinkResponse>(UserErrors.UserNotFound);

        if (!await IsAdminAsync(admin))
            return Result.Failure<CompanyMagicLoginLinkResponse>(new Error("User.Forbidden", "Only admins can generate magic login links.", StatusCodes.Status403Forbidden));

        var companyAccount = await _userManager.FindByIdAsync(companyAccountUserId.ToString());
        if (companyAccount is null)
            return Result.Failure<CompanyMagicLoginLinkResponse>(UserErrors.UserNotFound);

        var isCompanyAccount = await _userManager.IsInRoleAsync(companyAccount, DefaultRoles.PartnerCompany);
        if (!isCompanyAccount)
            return Result.Failure<CompanyMagicLoginLinkResponse>(new Error("User.InvalidRole", "Target user is not a company account.", StatusCodes.Status400BadRequest));

        var rawToken = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(48));
        var tokenHash = ComputeSha256(rawToken);
        var expiresOn = DateTime.UtcNow.AddMinutes(15);

        await _unitOfWork.Repository<CompanyMagicLinkToken>().AddAsync(new CompanyMagicLinkToken
        {
            UserId = companyAccountUserId,
            TokenHash = tokenHash,
            ExpiresOn = expiresOn,
            CreatedById = adminUserId
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin.ToString();
        var link = string.IsNullOrWhiteSpace(origin)
            ? $"/company-magic-login?token={rawToken}"
            : $"{origin}/company-magic-login?token={rawToken}";

        return Result.Success(new CompanyMagicLoginLinkResponse(
            companyAccountUserId,
            link,
            link,
            expiresOn
        ));
    }

    private async Task<string> GenerateEncodedResetTokenAsync(ApplicationUser user)
    {
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));
    }

    private static string BuildCompanyCode(string companyName)
    {
        var letters = new string(companyName.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        var prefix = letters.Length >= 6 ? letters[..6] : letters.PadRight(6, 'X');
        return $"{prefix}-{Guid.NewGuid():N}"[..20];
    }

    private static string BuildCompanyUserName(Guid companyId, string businessIdentifier)
    {
        var cleaned = new string(businessIdentifier.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        var prefix = cleaned.Length > 16 ? cleaned[..16] : cleaned;
        return $"cu-{companyId:N}-{prefix}";
    }

    private async Task<bool> IsAdminAsync(ApplicationUser user)
    {
        return await _userManager.IsInRoleAsync(user, DefaultRoles.Admin)
            || await _userManager.IsInRoleAsync(user, DefaultRoles.SystemAdmin);
    }

    private static string ExtractBusinessIdentifier(string userName, Guid companyId)
    {
        var prefix = $"cu-{companyId:N}-";
        if (userName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            var suffix = userName[prefix.Length..];
            return suffix.ToUpperInvariant();
        }

        return userName.ToUpperInvariant();
    }

    private static string ComputeSha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
