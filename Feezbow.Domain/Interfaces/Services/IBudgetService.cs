using Feezbow.Domain.Models.Budgets;

namespace Feezbow.Domain.Interfaces.Services;

public interface IBudgetService
{
    /// <summary>
    /// Aggregates bills for a single project over the given date range. Groups by currency since no FX conversion is applied.
    /// </summary>
    Task<ProjectBudgetSummary> GetProjectBudgetSummaryAsync(
        long projectId,
        long userId,
        DateTime from,
        DateTime to,
        int upcomingDays,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aggregates the current user's split obligations across all their projects within the given date range.
    /// </summary>
    Task<UserBudgetSummary> GetUserBudgetSummaryAsync(
        long userId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns per-month totals for a project's bills. Grouped by (year, month, currency).
    /// </summary>
    Task<ProjectBudgetTimeline> GetProjectBudgetTimelineAsync(
        long projectId,
        long userId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}
