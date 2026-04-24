using Feezbow.Domain.Models.Budgets;

namespace Feezbow.Application.Features.Budgets.GetProjectBudgetTimeline;

public record ProjectBudgetTimelinePointDto(
    int Year,
    int Month,
    string Currency,
    decimal TotalAmount,
    decimal PaidAmount);

public record ProjectBudgetTimelineDto(
    long ProjectId,
    DateTime FromDate,
    DateTime ToDate,
    IReadOnlyList<ProjectBudgetTimelinePointDto> Points)
{
    public static ProjectBudgetTimelineDto FromDomain(ProjectBudgetTimeline t) => new(
        t.ProjectId,
        t.FromDate,
        t.ToDate,
        t.Points.Select(p => new ProjectBudgetTimelinePointDto(
            p.Year, p.Month, p.Currency, p.TotalAmount, p.PaidAmount)).ToList());
}
