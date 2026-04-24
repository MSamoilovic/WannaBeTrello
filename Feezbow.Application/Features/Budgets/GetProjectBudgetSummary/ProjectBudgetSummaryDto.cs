using Feezbow.Application.Features.Bills.GetBillsByProject;
using Feezbow.Domain.Models.Budgets;

namespace Feezbow.Application.Features.Budgets.GetProjectBudgetSummary;

public record ProjectBudgetCurrencyOverviewDto(
    string Currency,
    decimal TotalAmount,
    decimal TotalPaid,
    decimal TotalUnpaid,
    decimal OverdueAmount,
    int OverdueCount);

public record ProjectBudgetCategoryBreakdownDto(
    string? Category,
    string Currency,
    decimal TotalAmount,
    decimal PaidAmount);

public record ProjectBudgetMemberBreakdownDto(
    long UserId,
    string? FirstName,
    string? LastName,
    string Currency,
    decimal Owed,
    decimal Paid,
    decimal Outstanding);

public record ProjectBudgetSummaryDto(
    long ProjectId,
    DateTime FromDate,
    DateTime ToDate,
    IReadOnlyList<ProjectBudgetCurrencyOverviewDto> ByCurrency,
    IReadOnlyList<ProjectBudgetCategoryBreakdownDto> ByCategory,
    IReadOnlyList<ProjectBudgetMemberBreakdownDto> ByMember,
    IReadOnlyList<BillDto> UpcomingBills,
    IReadOnlyList<BillDto> OverdueBills)
{
    public static ProjectBudgetSummaryDto FromDomain(ProjectBudgetSummary s) => new(
        s.ProjectId,
        s.FromDate,
        s.ToDate,
        s.ByCurrency.Select(c => new ProjectBudgetCurrencyOverviewDto(
            c.Currency, c.TotalAmount, c.TotalPaid, c.TotalUnpaid, c.OverdueAmount, c.OverdueCount)).ToList(),
        s.ByCategory.Select(c => new ProjectBudgetCategoryBreakdownDto(
            c.Category, c.Currency, c.TotalAmount, c.PaidAmount)).ToList(),
        s.ByMember.Select(m => new ProjectBudgetMemberBreakdownDto(
            m.UserId, m.FirstName, m.LastName, m.Currency, m.Owed, m.Paid, m.Outstanding)).ToList(),
        s.UpcomingBills.Select(BillDto.FromEntity).ToList(),
        s.OverdueBills.Select(BillDto.FromEntity).ToList());
}
