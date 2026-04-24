using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Budgets.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Budgets.GetProjectBudgetTimeline;

public class GetProjectBudgetTimelineQueryHandler(
    IBudgetService budgetService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetProjectBudgetTimelineQuery, ProjectBudgetTimelineDto>
{
    public async Task<ProjectBudgetTimelineDto> Handle(
        GetProjectBudgetTimelineQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var (from, to) = BudgetDateRange.ResolveRollingMonths(request.From, request.To, request.Months);

        var cached = await cacheService.GetOrSetAsync(
            CacheKeys.ProjectBudgetTimeline(request.ProjectId, from, to),
            async () =>
            {
                var timeline = await budgetService.GetProjectBudgetTimelineAsync(
                    request.ProjectId, userId, from, to, cancellationToken);
                return ProjectBudgetTimelineDto.FromDomain(timeline);
            },
            CacheExpiration.Medium,
            cancellationToken);

        return cached ?? ProjectBudgetTimelineDto.FromDomain(
            await budgetService.GetProjectBudgetTimelineAsync(
                request.ProjectId, userId, from, to, cancellationToken));
    }
}
