using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Budgets.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Budgets.GetUserBudgetSummary;

public class GetUserBudgetSummaryQueryHandler(
    IBudgetService budgetService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetUserBudgetSummaryQuery, UserBudgetSummaryDto>
{
    public async Task<UserBudgetSummaryDto> Handle(
        GetUserBudgetSummaryQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var (from, to) = BudgetDateRange.ResolveMonthly(request.From, request.To);

        var cached = await cacheService.GetOrSetAsync(
            CacheKeys.UserBudgetSummary(userId, from, to),
            async () =>
            {
                var summary = await budgetService.GetUserBudgetSummaryAsync(userId, from, to, cancellationToken);
                return UserBudgetSummaryDto.FromDomain(summary);
            },
            CacheExpiration.Short,
            cancellationToken);

        return cached ?? UserBudgetSummaryDto.FromDomain(
            await budgetService.GetUserBudgetSummaryAsync(userId, from, to, cancellationToken));
    }
}
