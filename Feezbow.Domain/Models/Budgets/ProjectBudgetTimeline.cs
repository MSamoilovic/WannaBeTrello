namespace Feezbow.Domain.Models.Budgets;

public record ProjectBudgetTimelinePoint(
    int Year,
    int Month,
    string Currency,
    decimal TotalAmount,
    decimal PaidAmount);

public record ProjectBudgetTimeline(
    long ProjectId,
    DateTime FromDate,
    DateTime ToDate,
    IReadOnlyList<ProjectBudgetTimelinePoint> Points);
