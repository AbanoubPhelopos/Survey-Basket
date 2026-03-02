using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Survey_Basket.Api.Models;
using Survey_Basket.Application.Abstractions.Const;
using Survey_Basket.Application.Contracts.Common;
using Survey_Basket.Application.Contracts.User;
using Survey_Basket.Application.Extensions;
using Survey_Basket.Application.Services.AuthServices.Filter;
using Survey_Basket.Application.Services.User;

namespace Survey_Basket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(IUserServices userServices, ILogger<UsersController> logger) : ControllerBase
{
    private readonly IUserServices _userServices = userServices;
    private readonly ILogger<UsersController> _logger = logger;

    [HttpGet("")]
    [HasPermission(Permissions.GetUsers)]
    public async Task<ActionResult<ServiceResult<ServiceListResult<UserResponse, UsersStatsResponse>>>> GetUsers([FromQuery] RequestFilters filters, [FromQuery] string? status, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _userServices.GetUsersFilterResultAsync(filters, status, cancellationToken);
            return result.IsSuccess
                ? Ok(ServiceResult<ServiceListResult<UserResponse, UsersStatsResponse>>.Success(result.Value))
                : Ok(ServiceResult<ServiceListResult<UserResponse, UsersStatsResponse>>.Failed(new ServiceError(result.Error.Message, int.TryParse(result.Error.Code, out var c) ? c : (result.Error.statusCode ?? 400))));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering users");
            return Ok(ServiceResult<ServiceListResult<UserResponse, UsersStatsResponse>>.Failed(new ServiceError(ex.Message, 500)));
        }
    }

    [HttpGet("stats")]
    [HasPermission(Permissions.GetUsers)]
    public async Task<IActionResult> GetUsersStats(CancellationToken cancellationToken)
    {
        var result = await _userServices.GetUsersStatsAsync(cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails();
    }

    [HttpPost("company-accounts")]
    [HasPermission(Permissions.ManageCompanies)]
    public async Task<IActionResult> CreateCompanyAccount([FromBody] CreateCompanyAccountRequest request, CancellationToken cancellationToken)
    {
        var result = await _userServices.CreateCompanyAccountAsync(User.GetUserId(), request, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails();
    }

    [HttpGet("company-accounts")]
    [HasPermission(Permissions.ManageCompanies)]
    public async Task<ActionResult<ServiceResult<ServiceListResult<CompanyAccountListItemResponse, CompanyAccountsStatsResponse>>>> GetCompanyAccounts([FromQuery] RequestFilters filters, [FromQuery] string? state, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _userServices.GetCompanyAccountsFilterResultAsync(User.GetUserId(), filters, state, cancellationToken);
            return result.IsSuccess
                ? Ok(ServiceResult<ServiceListResult<CompanyAccountListItemResponse, CompanyAccountsStatsResponse>>.Success(result.Value))
                : Ok(ServiceResult<ServiceListResult<CompanyAccountListItemResponse, CompanyAccountsStatsResponse>>.Failed(new ServiceError(result.Error.Message, int.TryParse(result.Error.Code, out var c) ? c : (result.Error.statusCode ?? 400))));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering company accounts");
            return Ok(ServiceResult<ServiceListResult<CompanyAccountListItemResponse, CompanyAccountsStatsResponse>>.Failed(new ServiceError(ex.Message, 500)));
        }
    }

    [HttpPut("company-accounts/{companyAccountUserId:guid}/lock-state")]
    [HasPermission(Permissions.ManageCompanies)]
    public async Task<IActionResult> SetCompanyAccountLockState([FromRoute] Guid companyAccountUserId, [FromQuery] bool locked, CancellationToken cancellationToken)
    {
        var result = await _userServices.SetCompanyAccountLockStateAsync(User.GetUserId(), companyAccountUserId, locked, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.ToProblemDetails();
    }

    [HttpPost("company-accounts/{companyAccountUserId:guid}/magic-login-link")]
    [HasPermission(Permissions.ManageCompanies)]
    public async Task<IActionResult> GenerateCompanyMagicLoginLink([FromRoute] Guid companyAccountUserId, CancellationToken cancellationToken)
    {
        var result = await _userServices.GenerateCompanyMagicLoginLinkAsync(User.GetUserId(), companyAccountUserId, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails();
    }

    [HttpPost("company-user-records")]
    public async Task<IActionResult> CreateCompanyUserRecord([FromBody] CreateCompanyUserRecordRequest request, CancellationToken cancellationToken)
    {
        var result = await _userServices.CreateCompanyUserRecordAsync(User.GetUserId(), request, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails();
    }

    [HttpGet("company-user-records")]
    public async Task<ActionResult<ServiceResult<ServiceListResult<CreateCompanyUserRecordResponse, CompanyUserRecordsStatsResponse>>>> GetCompanyUserRecords([FromQuery] RequestFilters filters, [FromQuery] string? identifierMode, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _userServices.GetCompanyUserRecordsFilterResultAsync(User.GetUserId(), filters, identifierMode, cancellationToken);
            return result.IsSuccess
                ? Ok(ServiceResult<ServiceListResult<CreateCompanyUserRecordResponse, CompanyUserRecordsStatsResponse>>.Success(result.Value))
                : Ok(ServiceResult<ServiceListResult<CreateCompanyUserRecordResponse, CompanyUserRecordsStatsResponse>>.Failed(new ServiceError(result.Error.Message, int.TryParse(result.Error.Code, out var c) ? c : (result.Error.statusCode ?? 400))));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering company user records");
            return Ok(ServiceResult<ServiceListResult<CreateCompanyUserRecordResponse, CompanyUserRecordsStatsResponse>>.Failed(new ServiceError(ex.Message, 500)));
        }
    }

    [HttpGet("admin/company-users")]
    [HasPermission(Permissions.ManageCompanies)]
    public async Task<ActionResult<ServiceResult<ServiceListResult<AdminCompanyUserListItemResponse, AdminCompanyUsersStatsResponse>>>> GetAdminCompanyUsers([FromQuery] RequestFilters filters, [FromQuery] Guid? companyId, [FromQuery] string? status, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _userServices.GetAdminCompanyUsersFilterResultAsync(User.GetUserId(), filters, companyId, status, cancellationToken);
            return result.IsSuccess
                ? Ok(ServiceResult<ServiceListResult<AdminCompanyUserListItemResponse, AdminCompanyUsersStatsResponse>>.Success(result.Value))
                : Ok(ServiceResult<ServiceListResult<AdminCompanyUserListItemResponse, AdminCompanyUsersStatsResponse>>.Failed(new ServiceError(result.Error.Message, int.TryParse(result.Error.Code, out var c) ? c : (result.Error.statusCode ?? 400))));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering admin company users");
            return Ok(ServiceResult<ServiceListResult<AdminCompanyUserListItemResponse, AdminCompanyUsersStatsResponse>>.Failed(new ServiceError(ex.Message, 500)));
        }
    }

    [HttpGet("company-user-records/stats")]
    public async Task<IActionResult> GetCompanyUserRecordsStats(CancellationToken cancellationToken)
    {
        var result = await _userServices.GetCompanyUserRecordsStatsAsync(User.GetUserId(), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails();
    }

    [HttpPost("company-user-invites")]
    [HasPermission(Permissions.ManageCompanyUsers)]
    public async Task<IActionResult> CreateCompanyUserInvite([FromBody] CreateCompanyUserInviteRequest request, CancellationToken cancellationToken)
    {
        var result = await _userServices.CreateCompanyUserInviteAsync(User.GetUserId(), request, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails();
    }

    [HttpGet("company-user-invites")]
    [HasPermission(Permissions.ManageCompanyUsers)]
    public async Task<IActionResult> GetCompanyUserInvites(CancellationToken cancellationToken)
    {
        var result = await _userServices.GetCompanyUserInvitesAsync(User.GetUserId(), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails();
    }

    [HttpPost("company-poll-access-links")]
    public async Task<IActionResult> CreateCompanyPollAccessLink([FromBody] CreateCompanyPollAccessLinkRequest request, CancellationToken cancellationToken)
    {
        var result = await _userServices.CreateCompanyPollAccessLinkAsync(User.GetUserId(), request, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails();
    }
}
