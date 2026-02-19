using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Survey_Basket.Application.Abstractions;
using Survey_Basket.Application.Abstractions.Const;
using Survey_Basket.Application.Contracts.User;
using Survey_Basket.Application.Errors;
using Survey_Basket.Domain.Abstractions;
using Survey_Basket.Domain.Entities;

namespace Survey_Basket.Application.Services.User;

public class UserServices(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork) : IUserServices
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager = userManager;


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
                .SetProperty(u => u.LastName, request.LastName), cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ChangePassword(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        var result = await _userManager.ChangePasswordAsync(user!, request.CurrentPassword, request.NewPassword);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));

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
            CreatedById = adminUserId
        };

        await _unitOfWork.Repository<Company>().AddAsync(company, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var companyAccount = new ApplicationUser
        {
            Email = request.ContactEmail.Trim(),
            UserName = request.ContactEmail.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            EmailConfirmed = true,
            IsDisabled = true
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
}
