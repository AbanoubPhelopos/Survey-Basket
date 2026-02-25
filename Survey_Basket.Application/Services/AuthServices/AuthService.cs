using System.Security.Cryptography;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Survey_Basket.Application.Abstractions;
using Survey_Basket.Application.Contracts.Authentication;
using Survey_Basket.Application.Contracts.User;
using Survey_Basket.Application.Errors;
using Survey_Basket.Application.Helpers;
using Survey_Basket.Domain.Abstractions;
using Survey_Basket.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Survey_Basket.Application.Abstractions.Const;

namespace Survey_Basket.Application.Services.AuthServices;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtProvider jwtProvider,
    ILogger<AuthService> logger,
    IEmailSender emailSender,
    IHttpContextAccessor httpContextAccessor,
    IUnitOfWork unitOfWork) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IJwtProvider _jwtProvider = jwtProvider;
    private readonly ILogger<AuthService> _logger = logger;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    private readonly int _refreshTokenExpiryDays = 14;

    public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (await _userManager.FindByEmailAsync(email) is not { } user)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        if (user.IsDisabled)
            return Result.Failure<AuthResponse>(UserErrors.DisabledUser);

        var result = await _signInManager.PasswordSignInAsync(user, password, false, true);

        if (result.Succeeded)
        {
            var response = await BuildAuthResponseAsync(user, cancellationToken);
            return Result.Success(response);
        }

        var error = result.IsNotAllowed
            ? UserErrors.EmailNotConfirmed
            : result.IsLockedOut
            ? UserErrors.LockedUser
            : UserErrors.InvalidCredentials;

        return Result.Failure<AuthResponse>(error);
    }

    public async Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        var userId = _jwtProvider.ValidateToken(token);

        if (userId is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidToken);

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidToken);

        if (user.IsDisabled)
            return Result.Failure<AuthResponse>(UserErrors.DisabledUser);

        if (user.LockoutEnd > DateTime.UtcNow)
            return Result.Failure<AuthResponse>(UserErrors.LockedUser);

        var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);

        if (userRefreshToken is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshToken);

        userRefreshToken.RevokedAt = DateTime.UtcNow;

        var response = await BuildAuthResponseAsync(user, cancellationToken);
        return Result.Success(response);
    }

    public async Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        var userId = _jwtProvider.ValidateToken(token);

        if (userId is null)
            return Result.Failure(UserErrors.InvalidToken);

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure(UserErrors.InvalidToken);

        var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);

        if (userRefreshToken is null)
            return Result.Failure(UserErrors.InvalidRefreshToken);

        userRefreshToken.RevokedAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        return Result.Success();
    }

    public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var emailIsExists = await _userManager.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);

        if (emailIsExists)
            return Result.Failure(UserErrors.EmailAlreadyExists);

        var user = request.Adapt<ApplicationUser>();

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            var addToRoleResult = await _userManager.AddToRoleAsync(user, DefaultRoles.Member);

            if (!addToRoleResult.Succeeded)
            {
                var roleError = addToRoleResult.Errors.First();
                return Result.Failure(new Error(roleError.Code, roleError.Description, StatusCodes.Status400BadRequest));
            }

            return Result.Success();
        }

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request)
    {
        if (await _userManager.FindByIdAsync(request.UserId) is not { } user)
            return Result.Failure(UserErrors.InvalidCode);

        if (user.EmailConfirmed)
            return Result.Failure(UserErrors.EmailAlreadyConfirmed);

        var code = request.Code;

        try
        {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            return Result.Failure(UserErrors.InvalidCode);
        }

        var result = await _userManager.ConfirmEmailAsync(user, code);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, DefaultRoles.Member);
            return Result.Success();
        }

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ResendConfirmEmailAsync(ResendConfirmationEmailRequest request)
    {
        if (await _userManager.FindByEmailAsync(request.Email) is not { } user)
            return Result.Success();

        if (user.EmailConfirmed)
            return Result.Failure(UserErrors.EmailAlreadyConfirmed);

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        _logger.LogInformation("Confirmation code: {code}", code);

        await SendConfirmationEmail(user, code);

        return Result.Success();
    }

    public async Task<Result> SendResetPasswordCode(string email)
    {
        if (await _userManager.FindByEmailAsync(email) is not { } user)
            return Result.Success();

        if (!user.EmailConfirmed)
            return Result.Failure(UserErrors.EmailNotConfirmed);

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        _logger.LogInformation("Reset code: {code}", code);

        await SendResetPasswordEmail(user, code);

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || !user.EmailConfirmed)
            return Result.Failure(UserErrors.InvalidCode);

        IdentityResult result;

        try
        {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
            result = await _userManager.ResetPasswordAsync(user, code, request.NewPassword);
        }
        catch (FormatException)
        {
            result = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
        }

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status401Unauthorized));
    }

    public async Task<Result> ActivateCompanyAccountAsync(ActivateCompanyAccountRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(request.CompanyAccountUserId.ToString());
        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(DefaultRoles.PartnerCompany))
            return Result.Failure(UserErrors.InvalidCredentials);

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.ActivationToken));
        }
        catch (FormatException)
        {
            return Result.Failure(UserErrors.InvalidCode);
        }

        var resetResult = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);
        if (!resetResult.Succeeded)
        {
            var error = resetResult.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        user.IsDisabled = false;
        await _userManager.UpdateAsync(user);

        return Result.Success();
    }

    public async Task<Result> RequestCompanyMagicLinkAsync(CompanyMagicLinkRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result.Success();

        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
            return Result.Success();

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(DefaultRoles.PartnerCompany))
            return Result.Success();

        var rawToken = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(48));
        var tokenHash = ComputeSha256(rawToken);

        await _unitOfWork.Repository<CompanyMagicLinkToken>().AddAsync(new CompanyMagicLinkToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresOn = DateTime.UtcNow.AddMinutes(15)
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin.ToString();
        var link = string.IsNullOrWhiteSpace(origin)
            ? $"/company-magic-login?token={rawToken}"
            : $"{origin}/company-magic-login?token={rawToken}";

        _logger.LogInformation("Company magic link generated for {Email}. Link: {Link}", user.Email, link);

        try
        {
            await _emailSender.SendEmailAsync(user.Email!, "Survey Basket: Company login link", $"Use this one-time link to sign in: {link}");
        }
        catch
        {
            // Fallback is log-only in local/dev.
        }

        return Result.Success();
    }

    public async Task<Result<AuthResponse>> RedeemCompanyMagicLinkAsync(CompanyMagicLinkRedeemRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return Result.Failure<AuthResponse>(UserErrors.InvalidToken);

        var tokenHash = ComputeSha256(request.Token.Trim());
        var token = await _unitOfWork.Repository<CompanyMagicLinkToken>()
            .GetAsync(x => x.TokenHash == tokenHash && x.UsedOn == null && x.RevokedOn == null, new[] { nameof(CompanyMagicLinkToken.User) }, cancellationToken);

        if (token is null || token.ExpiresOn < DateTime.UtcNow)
            return Result.Failure<AuthResponse>(UserErrors.InvalidToken);

        var user = token.User;
        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(DefaultRoles.PartnerCompany))
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        token.UsedOn = DateTime.UtcNow;
        user.IsDisabled = false;

        _unitOfWork.Repository<CompanyMagicLinkToken>().Update(token);
        await _userManager.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = await BuildAuthResponseAsync(user, cancellationToken);
        return Result.Success(response);
    }

    public async Task<Result<AuthResponse>> RedeemCompanyUserInviteAsync(CompanyUserInviteRedeemRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return Result.Failure<AuthResponse>(UserErrors.InvalidToken);

        var tokenHash = ComputeSha256(request.Token.Trim());
        var invite = await _unitOfWork.Repository<CompanyUserInvite>()
            .GetAsync(x => x.TokenHash == tokenHash && x.UsedOn == null && x.RevokedOn == null, cancellationToken: cancellationToken);

        if (invite is null || invite.ExpiresOn < DateTime.UtcNow)
            return Result.Failure<AuthResponse>(UserErrors.InvalidToken);

        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant();
        var mobile = string.IsNullOrWhiteSpace(request.Mobile) ? null : request.Mobile.Trim();

        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(mobile))
            return Result.Failure<AuthResponse>(new Error("Invite.IdentityRequired", "Email or mobile is required.", StatusCodes.Status400BadRequest));

        if (!string.IsNullOrWhiteSpace(invite.EmailHint) && !string.Equals(invite.EmailHint, email, StringComparison.OrdinalIgnoreCase))
            return Result.Failure<AuthResponse>(new Error("Invite.IdentityMismatch", "Invite identity does not match.", StatusCodes.Status400BadRequest));

        if (!string.IsNullOrWhiteSpace(invite.MobileHint) && !string.Equals(invite.MobileHint, mobile, StringComparison.OrdinalIgnoreCase))
            return Result.Failure<AuthResponse>(new Error("Invite.IdentityMismatch", "Invite identity does not match.", StatusCodes.Status400BadRequest));

        ApplicationUser? user = null;
        if (!string.IsNullOrWhiteSpace(email))
            user = await _userManager.FindByEmailAsync(email);

        if (user is null && !string.IsNullOrWhiteSpace(mobile))
            user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == mobile, cancellationToken);

        if (user is null)
        {
            user = new ApplicationUser
            {
                Email = email,
                UserName = email ?? $"mob-{mobile}",
                PhoneNumber = mobile,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                EmailConfirmed = !string.IsNullOrWhiteSpace(email),
                PhoneNumberConfirmed = !string.IsNullOrWhiteSpace(mobile),
                IsDisabled = false,
                ProfileCompleted = false,
                IsFirstLogin = true
            };

            var initialPassword = string.IsNullOrWhiteSpace(request.Password) ? $"Tmp!{Guid.NewGuid():N}aA1" : request.Password!;
            var createResult = await _userManager.CreateAsync(user, initialPassword);
            if (!createResult.Succeeded)
            {
                var createError = createResult.Errors.First();
                return Result.Failure<AuthResponse>(new Error(createError.Code, createError.Description, StatusCodes.Status400BadRequest));
            }
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (!currentRoles.Contains(DefaultRoles.CompanyUser))
        {
            var addRole = await _userManager.AddToRoleAsync(user, DefaultRoles.CompanyUser);
            if (!addRole.Succeeded)
            {
                var roleError = addRole.Errors.First();
                return Result.Failure<AuthResponse>(new Error(roleError.Code, roleError.Description, StatusCodes.Status400BadRequest));
            }
        }

        var linked = await _unitOfWork.Repository<CompanyUser>()
            .AnyAsync(x => x.CompanyId == invite.CompanyId && x.UserId == user.Id && x.IsActive, cancellationToken);
        if (!linked)
        {
            await _unitOfWork.Repository<CompanyUser>().AddAsync(new CompanyUser
            {
                CompanyId = invite.CompanyId,
                UserId = user.Id,
                IsPrimary = false,
                IsActive = true
            }, cancellationToken);
        }

        invite.UsedOn = DateTime.UtcNow;
        _unitOfWork.Repository<CompanyUserInvite>().Update(invite);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = await BuildAuthResponseAsync(user, cancellationToken);
        return Result.Success(response);
    }

    public async Task<Result<AuthResponse>> RedeemCompanyPollAccessAsync(CompanyPollAccessRedeemRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return Result.Failure<AuthResponse>(UserErrors.InvalidToken);

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            return Result.Failure<AuthResponse>(new Error("User.PasswordRequired", "Password is required and must be at least 6 characters.", StatusCodes.Status400BadRequest));

        var tokenHash = ComputeSha256(request.Token.Trim());
        var accessLink = await _unitOfWork.Repository<CompanyPollAccessLink>()
            .GetAsync(x => x.TokenHash == tokenHash && x.RevokedOn == null, cancellationToken: cancellationToken);

        if (accessLink is null || accessLink.ExpiresOn < DateTime.UtcNow)
            return Result.Failure<AuthResponse>(UserErrors.InvalidToken);

        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant();
        var mobile = string.IsNullOrWhiteSpace(request.Mobile) ? null : request.Mobile.Trim();
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(mobile))
            return Result.Failure<AuthResponse>(new Error("User.IdentityRequired", "Email or mobile is required.", StatusCodes.Status400BadRequest));

        ApplicationUser? user = null;
        if (!string.IsNullOrWhiteSpace(email))
            user = await _userManager.FindByEmailAsync(email);

        if (user is null && !string.IsNullOrWhiteSpace(mobile))
            user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == mobile, cancellationToken);

        if (user is null)
        {
            user = new ApplicationUser
            {
                Email = email,
                UserName = email ?? $"mob-{mobile}",
                PhoneNumber = mobile,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                EmailConfirmed = !string.IsNullOrWhiteSpace(email),
                PhoneNumberConfirmed = !string.IsNullOrWhiteSpace(mobile),
                IsDisabled = false,
                ProfileCompleted = true,
                IsFirstLogin = false
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var createError = createResult.Errors.First();
                return Result.Failure<AuthResponse>(new Error(createError.Code, createError.Description, StatusCodes.Status400BadRequest));
            }
        }
        else
        {
            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            if (!string.IsNullOrWhiteSpace(email)) user.Email = email;
            if (!string.IsNullOrWhiteSpace(mobile)) user.PhoneNumber = mobile;
            user.ProfileCompleted = true;
            user.IsFirstLogin = false;
            user.IsDisabled = false;

            if (await _userManager.HasPasswordAsync(user))
            {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, resetToken, request.Password);
            }
            else
            {
                await _userManager.AddPasswordAsync(user, request.Password);
            }

            await _userManager.UpdateAsync(user);
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (!currentRoles.Contains(DefaultRoles.CompanyUser))
            await _userManager.AddToRoleAsync(user, DefaultRoles.CompanyUser);

        var linkExists = await _unitOfWork.Repository<CompanyUser>()
            .AnyAsync(x => x.CompanyId == accessLink.CompanyId && x.UserId == user.Id && x.IsActive, cancellationToken);

        if (!linkExists)
        {
            await _unitOfWork.Repository<CompanyUser>().AddAsync(new CompanyUser
            {
                CompanyId = accessLink.CompanyId,
                UserId = user.Id,
                IsPrimary = false,
                IsActive = true
            }, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var response = await BuildAuthResponseAsync(user, cancellationToken, accessLink.PollId);
        return Result.Success(response);
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(ApplicationUser user, CancellationToken cancellationToken, Guid? redirectPollId = null)
    {
        var (userRoles, userPermissions) = await GetUserRolesAndPermissions(user, cancellationToken);

        var accountType = userRoles.Contains(DefaultRoles.PartnerCompany)
            ? "CompanyAccount"
            : userRoles.Contains(DefaultRoles.CompanyUser)
                ? "CompanyUserAccount"
                : "AdminAccount";

        var requiresActivation = userRoles.Contains(DefaultRoles.PartnerCompany) && user.IsDisabled;
        var requiresProfileCompletion = userRoles.Contains(DefaultRoles.CompanyUser) && !user.ProfileCompleted;
        var requiresPasswordSetup = user.IsFirstLogin;

        var (token, expiresIn) = _jwtProvider.GenerateToken(user, userRoles, userPermissions);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            ExpiresAt = refreshTokenExpiration
        });

        await _userManager.UpdateAsync(user);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            token,
            expiresIn,
            refreshToken,
            refreshTokenExpiration,
            userRoles,
            userPermissions,
            accountType,
            requiresActivation,
            requiresProfileCompletion,
            requiresPasswordSetup,
            redirectPollId
        );
    }

    private static string ComputeSha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    private async Task SendConfirmationEmail(ApplicationUser user, string code)
    {
        var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;

        var emailBody = EmailBodyBuilder.BuildEmailBody("EmailConfirmation",
            templateModel: new Dictionary<string, string>
            {
                { "{{name}}", user.FirstName },
                    { "{{action_url}}", $"{origin}/auth/emailConfirmation?userId={user.Id}&code={code}" }
            }
        );

        BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(user.Email!, "✅ Survey Basket: Email Confirmation", emailBody));

        await Task.CompletedTask;
    }

    private async Task SendResetPasswordEmail(ApplicationUser user, string code)
    {
        var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;

        var emailBody = EmailBodyBuilder.BuildEmailBody("ForgetPassword",
            templateModel: new Dictionary<string, string>
            {
                { "{{name}}", user.FirstName },
                { "{{action_url}}", $"{origin}/auth/forgetPassword?email={user.Email}&code={code}" }
            }
        );

        BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(user.Email!, "✅ Survey Basket: Change Password", emailBody));

        await Task.CompletedTask;
    }

    private async Task<(IEnumerable<string> roles, IEnumerable<string> permissions)> GetUserRolesAndPermissions(ApplicationUser user, CancellationToken cancellationToken)
    {
        var userRoles = (await _userManager.GetRolesAsync(user)).ToList();

        if (userRoles.Count == 0)
        {
            var addMemberRoleResult = await _userManager.AddToRoleAsync(user, DefaultRoles.Member);
            if (addMemberRoleResult.Succeeded)
            {
                userRoles.Add(DefaultRoles.Member);
            }
        }

        var userPermissions = userRoles.Count > 0
            ? await _unitOfWork.Roles.GetPermissionsByRolesAsync(userRoles, cancellationToken)
            : [];

        return (userRoles, userPermissions);
    }
}
