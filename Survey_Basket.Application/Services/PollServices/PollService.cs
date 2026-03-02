using Hangfire;
using Mapster;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;
using Survey_Basket.Application.Abstractions;
using Survey_Basket.Application.Contracts.Polls;
using Survey_Basket.Application.Errors;
using Survey_Basket.Application.Services.NotificationServices;
using Survey_Basket.Domain.Abstractions;
using Survey_Basket.Domain.Entities;
using Survey_Basket.Application.Contracts.Common;
using Survey_Basket.Application.Abstractions.Const;

namespace Survey_Basket.Application.Services.PollServices;

public class PollService(
    IUnitOfWork unitOfWork,
    INotificationService notificationService,
    IHttpContextAccessor httpContextAccessor) : IPollService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<Result> CreatePollAsync(CreatePollRequests poll, CancellationToken cancellationToken = default)
    {
        var userContext = GetCurrentUserContext();
        if (!userContext.IsSuccess)
            return Result.Failure(userContext.Error);

        var existingPoll = await _unitOfWork.Repository<Poll>()
            .AnyAsync(p => p.Title == poll.Title, cancellationToken);
        if (existingPoll)
            return Result.Failure(PollErrors.PollAlreadyExists);

        var targetCompanyIdsResult = await ResolveTargetCompanyIdsAsync(
            poll.TargetCompanyIds,
            userContext.Value.UserId,
            userContext.Value.Roles,
            cancellationToken,
            allowEmptyForAdmin: true);

        if (!targetCompanyIdsResult.IsSuccess)
            return Result.Failure(targetCompanyIdsResult.Error);

        var ownerCompanyId = targetCompanyIdsResult.Value.OwnerCompanyId;
        var targetCompanyIds = targetCompanyIdsResult.Value.TargetCompanyIds;

        var entity = new Poll
        {
            Title = poll.Title,
            Summary = poll.Summary,
            StartedAt = poll.StartedAt,
            EndedAt = poll.EndedAt,
            IsPublished = poll.IsPublished,
            OwnerCompanyId = ownerCompanyId,
            CreatedById = userContext.Value.UserId
        };

        await _unitOfWork.Repository<Poll>().AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _unitOfWork.Repository<PollOwner>().AddAsync(new PollOwner
        {
            PollId = entity.Id,
            UserId = userContext.Value.UserId,
            CompanyId = ownerCompanyId,
            CreatedById = userContext.Value.UserId
        }, cancellationToken);

        if (targetCompanyIds.Count > 0)
        {
            await _unitOfWork.Repository<PollAudience>().AddRangeAsync(
                targetCompanyIds.Select(companyId => new PollAudience
                {
                    PollId = entity.Id,
                    CompanyId = companyId,
                    CreatedById = userContext.Value.UserId
                }),
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeletePoll(Guid id, CancellationToken cancellationToken = default)
    {
        var existingPoll = await _unitOfWork.Repository<Poll>().GetByIdAsync(id, cancellationToken);
        if (existingPoll is null)
            return Result.Failure(PollErrors.PollNotFound);

        _unitOfWork.Repository<Poll>().Remove(existingPoll);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<PollResponse>> Get(Guid id, CancellationToken cancellationToken = default)
    {
        var poll = await _unitOfWork.Repository<Poll>()
            .GetAsync(x => x.Id == id, [nameof(Poll.Audiences)], cancellationToken: cancellationToken);

        if (poll is null)
            return Result.Failure<PollResponse>(PollErrors.PollNotFound);

        var targetCompanyIds = poll.Audiences.Select(x => x.CompanyId).Distinct().ToList();
        var response = new PollResponse(
            poll.Id,
            poll.Title,
            poll.Summary,
            poll.IsPublished,
            poll.StartedAt,
            poll.EndedAt,
            targetCompanyIds);

        return Result.Success(response);
    }

    public async Task<Result<PagedList<PollResponse>>> Get(RequestFilters filters, CancellationToken cancellationToken = default)
    {
        var userContext = GetCurrentUserContext();
        if (!userContext.IsSuccess)
            return Result.Failure<PagedList<PollResponse>>(userContext.Error);

        HashSet<Guid>? visiblePollIds = null;
        if (HasAnyRole(userContext.Value.Roles, DefaultRoles.PartnerCompany))
        {
            visiblePollIds = (await _unitOfWork.Repository<PollOwner>()
                .GetAllAsync(x => x.UserId == userContext.Value.UserId, cancellationToken))
                .Select(x => x.PollId)
                .ToHashSet();
        }

        var (polls, totalCount) = await _unitOfWork.Repository<Poll>().GetPagedAsync(
            filters.PageNumber,
            filters.PageSize,
            filters.SortColumn,
            filters.SortDirection,
            p => (visiblePollIds == null || visiblePollIds.Contains(p.Id))
                 && (string.IsNullOrEmpty(filters.SearchTerm) || p.Title.Contains(filters.SearchTerm)),
            cancellationToken);

        var pollResponses = polls.Adapt<IEnumerable<PollResponse>>();
        var result = new PagedList<PollResponse>(pollResponses, filters.PageNumber, totalCount, filters.PageSize);

        return Result.Success(result);
    }

    public async Task<Result<ServiceListResult<PollResponse, PollStatsResponse>>> GetFilterResult(RequestFilters filters, string? status, Guid userId, IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        var visiblePollIds = await ResolveVisiblePollIdsAsync(userId, roles, cancellationToken);

        var normalizedStatus = status?.Trim().ToLowerInvariant();

        var (polls, totalCount) = await _unitOfWork.Repository<Poll>().GetPagedAsync(
            filters.PageNumber,
            filters.PageSize,
            filters.SortColumn,
            filters.SortDirection,
            p => (visiblePollIds == null || visiblePollIds.Contains(p.Id))
                 && (string.IsNullOrEmpty(filters.SearchTerm) || p.Title.Contains(filters.SearchTerm))
                 && (string.IsNullOrWhiteSpace(normalizedStatus)
                     || normalizedStatus == "all"
                     || (normalizedStatus == "active" && p.IsPublished)
                     || (normalizedStatus == "draft" && !p.IsPublished)),
            cancellationToken);

        var paged = new PagedList<PollResponse>(polls.Adapt<IEnumerable<PollResponse>>(), filters.PageNumber, totalCount, filters.PageSize);
        var statsResult = await GetStats(userId, roles, cancellationToken);
        if (!statsResult.IsSuccess)
            return Result.Failure<ServiceListResult<PollResponse, PollStatsResponse>>(statsResult.Error);

        return Result.Success(new ServiceListResult<PollResponse, PollStatsResponse>(paged, statsResult.Value));
    }

    public async Task<Result<PollStatsResponse>> GetStats(Guid userId, IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        var visiblePollIds = await ResolveVisiblePollIdsAsync(userId, roles, cancellationToken);

        var polls = await _unitOfWork.Repository<Poll>()
            .GetAllAsync(x => visiblePollIds == null || visiblePollIds.Contains(x.Id), cancellationToken);

        var pollIds = polls.Select(x => x.Id).ToHashSet();

        var totalPolls = polls.Count();
        var activePolls = polls.Count(x => x.IsPublished);
        var draftPolls = totalPolls - activePolls;

        var votes = await _unitOfWork.Repository<Vote>()
            .GetAllAsync(x => pollIds.Contains(x.PollId), cancellationToken);

        var voteIds = votes.Select(x => x.Id).ToHashSet();

        var answersCount = voteIds.Count == 0
            ? 0
            : (await _unitOfWork.Repository<VoteAnswers>()
                .GetAllAsync(x => voteIds.Contains(x.VoteId), cancellationToken)).Count();

        return Result.Success(new PollStatsResponse(
            totalPolls,
            activePolls,
            draftPolls,
            votes.Count(),
            answersCount));
    }

    public async Task<Result<IEnumerable<PollResponse>>> GetCurrent(CancellationToken cancellationToken = default)
    {
        var userContext = GetCurrentUserContext();
        if (!userContext.IsSuccess)
            return Result.Failure<IEnumerable<PollResponse>>(userContext.Error);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        IEnumerable<Poll> polls;

        if (HasAnyRole(userContext.Value.Roles, DefaultRoles.Admin, DefaultRoles.SystemAdmin))
        {
            polls = await _unitOfWork.Repository<Poll>()
                .GetAllAsync(p => p.IsPublished && p.StartedAt <= today
                                && (!p.EndedAt.HasValue || p.EndedAt >= today), cancellationToken);
        }
        else
        {
            var companyIds = (await _unitOfWork.Repository<CompanyUser>()
                .GetAllAsync(x => x.UserId == userContext.Value.UserId && x.IsActive, cancellationToken))
                .Select(x => x.CompanyId)
                .ToHashSet();

            if (companyIds.Count == 0)
                return Result.Success(Enumerable.Empty<PollResponse>());

            var pollIds = (await _unitOfWork.Repository<PollAudience>()
                .GetAllAsync(x => companyIds.Contains(x.CompanyId), cancellationToken))
                .Select(x => x.PollId)
                .Distinct()
                .ToHashSet();

            polls = await _unitOfWork.Repository<Poll>()
                .GetAllAsync(p => pollIds.Contains(p.Id) && p.IsPublished && p.StartedAt <= today
                                && (!p.EndedAt.HasValue || p.EndedAt >= today), cancellationToken);
        }

        var response = polls.Adapt<IEnumerable<PollResponse>>();
        return Result.Success(response);
    }

    public async Task<Result> UpdatePoll(Guid id, UpdatePollRequests updatedPoll, CancellationToken cancellationToken = default)
    {
        var userContext = GetCurrentUserContext();
        if (!userContext.IsSuccess)
            return Result.Failure(userContext.Error);

        var existingPoll = await _unitOfWork.Repository<Poll>().GetByIdAsync(id, cancellationToken);

        if (existingPoll is null)
            return Result.Failure(PollErrors.PollNotFound);

        var canManageResult = await CanManagePollAsync(existingPoll, userContext.Value.UserId, userContext.Value.Roles, cancellationToken);
        if (!canManageResult.IsSuccess)
            return Result.Failure(canManageResult.Error);

        var pollWithSameTitle = await _unitOfWork.Repository<Poll>()
            .AnyAsync(p => p.Title == updatedPoll.Title && p.Id != id, cancellationToken);
        if (pollWithSameTitle)
            return Result.Failure(PollErrors.PollAlreadyExists);

        existingPoll.Title = updatedPoll.Title;
        existingPoll.Summary = updatedPoll.Summary;
        existingPoll.IsPublished = updatedPoll.IsPublished;
        existingPoll.StartedAt = updatedPoll.StartedAt;
        existingPoll.EndedAt = updatedPoll.EndedAt;
        existingPoll.UpdatedById = userContext.Value.UserId;

        _unitOfWork.Repository<Poll>().Update(existingPoll);

        var targetCompanyIdsResult = await ResolveTargetCompanyIdsAsync(
            updatedPoll.TargetCompanyIds,
            userContext.Value.UserId,
            userContext.Value.Roles,
            cancellationToken,
            allowEmptyForAdmin: true);

        if (!targetCompanyIdsResult.IsSuccess)
            return Result.Failure(targetCompanyIdsResult.Error);

        if (targetCompanyIdsResult.Value.TargetCompanyIds is { Count: > 0 })
        {
            await ReplaceAudienceAsync(id, targetCompanyIdsResult.Value.TargetCompanyIds, userContext.Value.UserId, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> AssignAudienceAsync(Guid pollId, AssignPollAudienceRequest request, CancellationToken cancellationToken = default)
    {
        var userContext = GetCurrentUserContext();
        if (!userContext.IsSuccess)
            return Result.Failure(userContext.Error);

        if (!HasAnyRole(userContext.Value.Roles, DefaultRoles.Admin, DefaultRoles.SystemAdmin))
            return Result.Failure(PollErrors.PollAccessDenied);

        var poll = await _unitOfWork.Repository<Poll>().GetByIdAsync(pollId, cancellationToken);
        if (poll is null)
            return Result.Failure(PollErrors.PollNotFound);

        var companyIds = request.CompanyIds?.Distinct().ToList() ?? [];
        var companies = await _unitOfWork.Repository<Company>()
            .GetAllAsync(x => companyIds.Contains(x.Id) && x.IsActive, cancellationToken);

        if (companies.Count() != companyIds.Count)
            return Result.Failure(PollErrors.InvalidTargetCompanies);

        await ReplaceAudienceAsync(pollId, companyIds, userContext.Value.UserId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> TogglePublishStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var poll = await _unitOfWork.Repository<Poll>().GetByIdAsync(id, cancellationToken);

        if (poll is null)
            return Result.Failure(PollErrors.PollNotFound);

        poll.IsPublished = !poll.IsPublished;
        _unitOfWork.Repository<Poll>().Update(poll);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (poll.IsPublished && poll.StartedAt == DateOnly.FromDateTime(DateTime.UtcNow))
            BackgroundJob.Enqueue(() => _notificationService.SendNewPollsNotification(poll.Id));

        return Result.Success();
    }

    private Result<(Guid UserId, List<string> Roles)> GetCurrentUserContext()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity is not { IsAuthenticated: true })
            return Result.Failure<(Guid, List<string>)>(PollErrors.PollAccessDenied);

        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Result.Failure<(Guid, List<string>)>(PollErrors.PollAccessDenied);

        return Result.Success((userId, ReadRoles(user)));
    }

    private static List<string> ReadRoles(ClaimsPrincipal user)
    {
        var roles = new List<string>();

        var rolesClaim = user.Claims.FirstOrDefault(x => x.Type == "roles")?.Value;
        if (!string.IsNullOrWhiteSpace(rolesClaim))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<List<string>>(rolesClaim);
                if (parsed is { Count: > 0 })
                    roles.AddRange(parsed.Where(x => !string.IsNullOrWhiteSpace(x))!);
            }
            catch
            {
                roles.Add(rolesClaim);
            }
        }

        var roleClaims = user.Claims
            .Where(x => x.Type == ClaimTypes.Role || x.Type == "role")
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x));

        roles.AddRange(roleClaims);

        return roles
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool HasAnyRole(IEnumerable<string> roles, params string[] expected)
    {
        var set = roles.Select(x => x.ToLowerInvariant()).ToHashSet();
        return expected.Any(x => set.Contains(x.ToLowerInvariant()));
    }

    private async Task<Result> CanManagePollAsync(Poll poll, Guid userId, IEnumerable<string> roles, CancellationToken cancellationToken)
    {
        if (HasAnyRole(roles, DefaultRoles.Admin, DefaultRoles.SystemAdmin))
            return Result.Success();

        // Backward-compatible ownership checks for polls created before PollOwner was introduced.
        if (poll.CreatedById == userId)
            return Result.Success();

        var companyIds = (await _unitOfWork.Repository<CompanyUser>()
            .GetAllAsync(x => x.UserId == userId && x.IsActive, cancellationToken))
            .Select(x => x.CompanyId)
            .ToHashSet();

        if (poll.OwnerCompanyId.HasValue && companyIds.Contains(poll.OwnerCompanyId.Value))
            return Result.Success();

        if (HasAnyRole(roles, DefaultRoles.PartnerCompany) && companyIds.Count > 0)
        {
            var isTargetedToCompany = await _unitOfWork.Repository<PollAudience>()
                .AnyAsync(x => x.PollId == poll.Id && companyIds.Contains(x.CompanyId), cancellationToken);

            if (isTargetedToCompany)
                return Result.Success();
        }

        var ownsPoll = await _unitOfWork.Repository<PollOwner>()
            .AnyAsync(x => x.PollId == poll.Id && x.UserId == userId, cancellationToken);

        return ownsPoll ? Result.Success() : Result.Failure(PollErrors.PollAccessDenied);
    }

    private async Task<Result<(Guid? OwnerCompanyId, List<Guid> TargetCompanyIds)>> ResolveTargetCompanyIdsAsync(
        IEnumerable<Guid>? requestedCompanyIds,
        Guid userId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken,
        bool allowEmptyForAdmin = false)
    {
        var companyIds = requestedCompanyIds?.Distinct().ToList() ?? [];

        if (HasAnyRole(roles, DefaultRoles.PartnerCompany))
        {
            var companyMembership = await _unitOfWork.Repository<CompanyUser>()
                .GetAsync(x => x.UserId == userId && x.IsActive, cancellationToken: cancellationToken);

            if (companyMembership is null)
                return Result.Failure<(Guid?, List<Guid>)>(PollErrors.PartnerCompanyNotLinked);

            return Result.Success<(Guid?, List<Guid>)>((companyMembership.CompanyId, [companyMembership.CompanyId]));
        }

        if (!HasAnyRole(roles, DefaultRoles.Admin, DefaultRoles.SystemAdmin))
            return Result.Failure<(Guid?, List<Guid>)>(PollErrors.PollAccessDenied);

        if (!allowEmptyForAdmin && companyIds.Count == 0)
            return Result.Failure<(Guid?, List<Guid>)>(PollErrors.InvalidTargetCompanies);

        if (companyIds.Count > 0)
        {
            var companies = await _unitOfWork.Repository<Company>()
                .GetAllAsync(x => companyIds.Contains(x.Id) && x.IsActive, cancellationToken);

            if (companies.Count() != companyIds.Count)
                return Result.Failure<(Guid?, List<Guid>)>(PollErrors.InvalidTargetCompanies);
        }

        return Result.Success<(Guid?, List<Guid>)>((null, companyIds));
    }

    private async Task ReplaceAudienceAsync(Guid pollId, IEnumerable<Guid> companyIds, Guid actorId, CancellationToken cancellationToken)
    {
        var existingAudience = await _unitOfWork.Repository<PollAudience>()
            .GetAllAsync(x => x.PollId == pollId, cancellationToken);

        _unitOfWork.Repository<PollAudience>().RemoveRange(existingAudience);

        var newAudience = companyIds.Select(companyId => new PollAudience
        {
            PollId = pollId,
            CompanyId = companyId,
            CreatedById = actorId
        });

        await _unitOfWork.Repository<PollAudience>().AddRangeAsync(newAudience, cancellationToken);
    }

    private async Task<HashSet<Guid>?> ResolveVisiblePollIdsAsync(Guid userId, IEnumerable<string> roles, CancellationToken cancellationToken)
    {
        var roleList = roles.ToList();

        if (HasAnyRole(roleList, DefaultRoles.Admin, DefaultRoles.SystemAdmin))
            return null;

        if (HasAnyRole(roleList, DefaultRoles.PartnerCompany))
        {
            var ownerPollIds = (await _unitOfWork.Repository<PollOwner>()
                .GetAllAsync(x => x.UserId == userId, cancellationToken))
                .Select(x => x.PollId)
                .ToHashSet();

            var companyIds = (await _unitOfWork.Repository<CompanyUser>()
                .GetAllAsync(x => x.UserId == userId && x.IsActive, cancellationToken))
                .Select(x => x.CompanyId)
                .ToHashSet();

            var legacyOwnerPollIds = (await _unitOfWork.Repository<Poll>()
                .GetAllAsync(x => x.CreatedById == userId || (x.OwnerCompanyId.HasValue && companyIds.Contains(x.OwnerCompanyId.Value)), cancellationToken))
                .Select(x => x.Id)
                .ToHashSet();

            ownerPollIds.UnionWith(legacyOwnerPollIds);

            if (companyIds.Count > 0)
            {
                var partnerAudiencePollIds = (await _unitOfWork.Repository<PollAudience>()
                    .GetAllAsync(x => companyIds.Contains(x.CompanyId), cancellationToken))
                    .Select(x => x.PollId)
                    .ToHashSet();

                ownerPollIds.UnionWith(partnerAudiencePollIds);
            }

            return ownerPollIds;
        }

        if (!HasAnyRole(roleList, DefaultRoles.CompanyUser, DefaultRoles.Member))
            return null;

        var memberCompanyIds = (await _unitOfWork.Repository<CompanyUser>()
            .GetAllAsync(x => x.UserId == userId && x.IsActive, cancellationToken))
            .Select(x => x.CompanyId)
            .ToHashSet();

        if (memberCompanyIds.Count == 0)
            return [];

        var audiencePollIds = (await _unitOfWork.Repository<PollAudience>()
            .GetAllAsync(x => memberCompanyIds.Contains(x.CompanyId), cancellationToken))
            .Select(x => x.PollId)
            .ToHashSet();

        return audiencePollIds;
    }
}
