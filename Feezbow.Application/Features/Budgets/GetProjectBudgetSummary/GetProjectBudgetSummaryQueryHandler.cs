using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Budgets.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Budgets.GetProjectBudgetSummary;

public class GetProjectBudgetSummaryQueryHandler(
    IBudgetService budgetService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetProjectBudgetSummaryQuery, ProjectBudgetSummaryDto>
{
    public async Task<ProjectBudgetSummaryDto> Handle(
        GetProjectBudgetSummaryQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var (from, to) = BudgetDateRange.ResolveMonthly(request.From, request.To);

        var cached = await cacheService.GetOrSetAsync(
            CacheKeys.ProjectBudgetSummary(request.ProjectId, from, to, request.UpcomingDays),
            async () =>
            {
                var summary = await budgetService.GetProjectBudgetSummaryAsync(
                    request.ProjectId, userId, from, to, request.UpcomingDays, cancellationToken);
                return ProjectBudgetSummaryDto.FromDomain(summary);
            },
            CacheExpiration.Short,
            cancellationToken);

        return cached ?? ProjectBudgetSummaryDto.FromDomain(
            await budgetService.GetProjectBudgetSummaryAsync(
                request.ProjectId, userId, from, to, request.UpcomingDays, cancellationToken));
    }
}
